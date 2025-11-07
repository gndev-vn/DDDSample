using Mediator;

namespace Shared.Common;

public interface IPagedQuery<out T> : IRequest<T> where T : class
{
    int Page { get; }

    int PageSize { get; }
}