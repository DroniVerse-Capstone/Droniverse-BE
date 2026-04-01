using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class QuizQuestionAttemptRepository : MySqlRepository<QuizQuestionAttempt>, IQuizQuestionAttemptRepository
{
    public QuizQuestionAttemptRepository(MySqlDbContext context) : base(context)
    {
    }
}
