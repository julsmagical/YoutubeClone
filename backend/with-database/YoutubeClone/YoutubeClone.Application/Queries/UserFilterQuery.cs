using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Domain.Database.SqlServer.Entities;

namespace YoutubeClone.Application.Queries
{
    public static class UserFilterQuery
    {
        public static IQueryable<UserAccount> ApplyQuery(this IQueryable<UserAccount> queryable, FilterUserRequest model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                queryable = queryable.Where(x => x.UserName.Contains(model.UserName ?? ""));
            }
            if (!string.IsNullOrWhiteSpace(model.DisplayName))
            {
                queryable = queryable.Where(x => x.DisplayName.Contains(model.DisplayName ?? ""));
            }
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                queryable = queryable.Where(x => x.Email.Contains(model.Email ?? ""));
            }
            if (!string.IsNullOrWhiteSpace(model.Location))
            {
                queryable = queryable.Where(x => x.Location.Contains(model.Location ?? ""));
            }

            return queryable;
        }
    }
}
