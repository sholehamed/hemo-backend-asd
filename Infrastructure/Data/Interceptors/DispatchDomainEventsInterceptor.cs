using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly EntityStateTracker _stateTracker = new EntityStateTracker();

    public DispatchDomainEventsInterceptor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleEntityStateEvents(eventData.Context).Wait();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
       await HandleEntityStateEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        HandlePostSaveEvents(eventData.Context).Wait();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
      await  HandlePostSaveEvents(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task HandleEntityStateEvents(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    await entry.Entity.OnAdd(context);
                    _stateTracker.Add(entry.Entity, EntityState.Added);
                    break;
                case EntityState.Modified:
                    await entry.Entity.OnModified(context);
                    _stateTracker.Add(entry.Entity, EntityState.Modified);
                    break;
                case EntityState.Deleted:
                    await entry.Entity.OnDeleted(context);
                    _stateTracker.Add(entry.Entity, EntityState.Deleted);
                    break;
            }
        }
    }

    private async Task HandlePostSaveEvents(DbContext? context)
    {
        if (context == null) return;

        foreach (var entityStateInfo in _stateTracker.AllEntities)
        {
            var entity = entityStateInfo.Entity;
            var state = entityStateInfo.State;

            try
            {
                switch (state)
                {
                    case EntityState.Added:
                    {
                      await  entity.OnAddAfterSave(context);
                        break;
                    }
                    case EntityState.Modified:
                    {
                      await  entity.OnModifiedAfterSave(context);
                        break;
                    }
                    case EntityState.Deleted:
                    {
                     await   entity.OnDeletedAfterSave(context);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        _stateTracker.Clear();
    }
}

public class EntityStateTracker
{
    public List<EntityStateInfo> Entities { get; } = new List<EntityStateInfo>();

    public IEnumerable<EntityStateInfo> AllEntities => Entities;

    public void Add(BaseEntity entity, EntityState state) => Entities.Add(new EntityStateInfo(entity, state));

    public void Clear() => Entities.Clear();
}

public class EntityStateInfo
{
    public BaseEntity Entity { get; }
    public EntityState State { get; }

    public EntityStateInfo(BaseEntity entity, EntityState state)
    {
        Entity = entity;
        State = state;
    }
}