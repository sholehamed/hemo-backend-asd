using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public abstract class BaseEvent
{
    public virtual async Task OnAdd(DbContext Context)
    {

    }

    public virtual async Task OnModified(DbContext Context)
    {
    }

    public virtual async Task OnDeleted(DbContext Context)
    {
    }

    public virtual async Task OnAddAfterSave(DbContext Context)
    {
    }

    public virtual async Task OnModifiedAfterSave(DbContext Context)
    {
    }

    public virtual async Task OnDeletedAfterSave(DbContext Context)
    {
    }
}