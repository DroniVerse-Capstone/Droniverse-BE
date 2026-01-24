using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class QuizAnswerRepository : MySqlRepository<QuizAnswer>, IQuizAnswerRepository
{
    public QuizAnswerRepository(MySqlDbContext context) : base(context)
    {
    }
}

