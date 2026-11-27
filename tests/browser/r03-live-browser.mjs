import { chromium } from 'playwright';
import assert from 'node:assert/strict';

const base = 'http://127.0.0.1:1002';
const chunks=[];
for await (const chunk of process.stdin) chunks.push(chunk);
const sessions=JSON.parse(Buffer.concat(chunks).toString('utf8'));
const browser=await chromium.launch({headless:true});
let scenario='startup';
const contexts=[];
async function open(name) {
  const session=sessions[name];
  assert.ok(session?.token&&session?.role>0);
  const context=await browser.newContext({viewport:{width:1440,height:960}});
  contexts.push(context);
  // Block all external network access. Never intercept or synthesize local API responses.
  await context.route('**/*',route=>new URL(route.request().url()).origin===base?route.continue():route.abort());
  await context.addInitScript(({token,role})=>{
    if(location.origin!=='http://127.0.0.1:1002')return;
    sessionStorage.setItem('Token',token);
    sessionStorage.setItem('UserRole',String(role));
    sessionStorage.setItem('userRole',String(role));
    sessionStorage.setItem('firstName','Synthetic');
    sessionStorage.setItem('lastName','Browser');
  },session);
  const page=await context.newPage();
  page.setDefaultTimeout(20000);
  const responses=[],errors=[];
  page.on('response',r=>{
    const url=new URL(r.url());
    if(url.origin===base&&url.pathname.startsWith('/Identity/'))
      responses.push({path:url.pathname,method:r.request().method(),status:r.status()});
  });
  page.on('pageerror',()=>errors.push('uncaught browser exception'));
  return {context,page,responses,errors};
}
async function waitApi(page,path,method='GET',status=200){
  return page.waitForResponse(r=>new URL(r.url()).pathname===path&&r.request().method()===method&&r.status()===status);
}
async function loaded(page,selector){
  await page.locator(selector).first().waitFor({state:'visible'});
}
async function assertText(locator,expected){
  await locator.first().waitFor({state:'visible'});
  assert.ok((await locator.first().textContent()).includes(expected),'Expected visible fixture text');
}
function successful(responses,path,method='GET',status=200){
  return responses.some(r=>r.path===path&&r.method===method&&r.status===status);
}
try {
  scenario='manager-dashboard';
  const manager=await open('hemo.dashboard.manager');
  const mp=manager.page;
  await mp.goto(base+'/hemo/dashboard');
  await assertText(mp.locator('h1'),'داشبورد نقش سازمانی');
  for(const module of ['admission','requests','bank','transfer','hv','equipment','quality'])
    await loaded(mp,'[data-module="'+module+'"] .dashboard-metric strong');
  assert.equal(await mp.locator('.module-panel').count(),7);
  assert.equal(await mp.locator('.module-panel [role="alert"]').count(),0);
  for(const path of ['PatientAdmission/Stats','BloodRequest/Stats','BloodBank/Stats','Transfusion/Stats','Hemovigilance/Stats','HemoQuality/EquipmentStats','HemoQuality/Stats'])
    assert.ok(successful(manager.responses,'/Identity/'+path,'POST'),'Missing actual aggregate: '+path);
  assert.ok(successful(manager.responses,'/Identity/RoleAccess/CheckPagePermission','POST'));
  await mp.getByRole('button',{name:'بررسی مسیرهای مجاز'}).click();
  await mp.getByText('فقط مسیرهای تأییدشده نمایش داده می‌شوند.').waitFor();
  assert.equal(await mp.locator('a[href="/admission/new"],a[href="/admission/list"]').count(),0);
  assert.ok(!manager.responses.some(r=>r.path==='/Identity/PatientAdmission/BindPaging'));
  console.log('PASS manager seven live aggregates and authorized links');

  scenario='manager-denied-deep-link';
  await mp.goto(base+'/admission/new');
  await mp.waitForURL('**/auth/forbidden');
  assert.equal(await mp.locator('[formcontrolname="hisNumber"]').count(),0);
  console.log('PASS manager denied create route');

  scenario='admission-create';
  const admission=await open('hemo.admission');
  const ap=admission.page;
  await ap.goto(base+'/admission/new');
  await assertText(ap.locator('h1'),'تشکیل پرونده و پذیرش بیمار جدید');
  await ap.locator('[formcontrolname="identificationType"]').selectOption('Unknown');
  const his='FIX-BROWSER-'+Date.now();
  for(const [name,value] of Object.entries({firstName:'Synthetic',lastName:'Browser',hisNumber:his,ward:'BrowserFixture',bedNumber:'B1',physician:'Fixture'}))
    await ap.locator('[formcontrolname="'+name+'"]').fill(value);
  const created=waitApi(ap,'/Identity/PatientAdmission','POST',201);
  await ap.getByRole('button',{name:'ثبت نهایی پرونده و پذیرش'}).click();
  const id=await (await created).json();
  assert.match(id,/^[0-9a-f-]{36}$/i);
  await ap.waitForURL('**/admission/detail/'+id+'?created=1');
  await assertText(ap.locator('h1'),'پرونده پذیرش و مچ‌بند');
  await ap.getByText(his,{exact:true}).first().waitFor();
  assert.ok(successful(admission.responses,'/Identity/PatientAdmission/'+id));
  console.log('PASS create real admission and load persisted detail');

  scenario='admission-search';
  await ap.goto(base+'/admission/list');
  await ap.locator('input[name="search"]').fill(his);
  const search=waitApi(ap,'/Identity/PatientAdmission/BindPaging','POST');
  await ap.getByRole('button',{name:'جستجو',exact:true}).click();
  const result=await (await search).json();
  assert.ok(result.data.some(row=>row.guid===id));
  await ap.getByText(his,{exact:true}).first().waitFor();
  console.log('PASS persisted admission appears in real search');

  scenario='read-only-role';
  const viewer=await open('hemo.viewer');
  await viewer.page.goto(base+'/admission/list');
  await assertText(viewer.page.locator('h1'),'فهرست بیماران بستری');
  await loaded(viewer.page,'app-admission-table');
  assert.equal(await viewer.page.locator('a[href="/admission/new"]').count(),0);
  await viewer.page.goto(base+'/admission/new');
  await viewer.page.waitForURL('**/auth/forbidden');
  console.log('PASS read-only role denied admission mutation route');

  scenario='layout-and-runtime';
  for(const item of [manager,admission,viewer]){
    assert.ok(successful(item.responses,'/Identity/Menu/MenuBindUserMenu'),'Actual menu API missing');
    assert.ok(successful(item.responses,'/Identity/UserRole/GetCurrentUserRoles'),'Actual current roles API missing');
    assert.equal(item.errors.length,0,'Unexpected browser exception');
    assert.ok(!item.responses.some(r=>r.status>=500),'Server error during browser flow');
  }
  console.log('R03_BROWSER_PASS');
}catch{
  // Never dump DOM, headers, sessionStorage, response bodies or tokens to CI logs.
  console.error('R03 browser failed at '+scenario);
  process.exitCode=1;
}finally{
  for(const context of contexts)await context.close();
  await browser.close();
}
