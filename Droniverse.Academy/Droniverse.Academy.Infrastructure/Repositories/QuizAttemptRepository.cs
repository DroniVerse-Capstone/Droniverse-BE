using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class QuizAttemptRepository : MySqlRepository<QuizAttempt>, IQuizAttemptRepository
{
    public QuizAttemptRepository(MySqlDbContext context) : base(context)
    {
    }
}
