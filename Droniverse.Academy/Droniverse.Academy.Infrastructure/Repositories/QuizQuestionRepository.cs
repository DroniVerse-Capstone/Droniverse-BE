using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class QuizQuestionRepository : MySqlRepository<QuizQuestion>, IQuizQuestionRepository
{
    public QuizQuestionRepository(MySqlDbContext context) : base(context)
    {
    }
}

