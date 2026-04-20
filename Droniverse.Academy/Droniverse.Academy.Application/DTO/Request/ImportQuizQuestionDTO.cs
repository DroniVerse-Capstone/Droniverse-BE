using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Request
{
    public class ImportQuizQuestionDTO
    {
        public string ContentVN { get; set; } = string.Empty;
        public string ContentEN { get; set; } = string.Empty;
        public string AnswerA { get; set; } = string.Empty;
        public string AnswerB { get; set; } = string.Empty;
        public string AnswerC { get; set; } = string.Empty;
        public string AnswerD { get; set; } = string.Empty;
        public string AnswerA_EN { get; set; } = string.Empty;
        public string AnswerB_EN { get; set; } = string.Empty;
        public string AnswerC_EN { get; set; } = string.Empty;
        public string AnswerD_EN { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public float Score { get; set; } //float
        public Guid QuizID { get; set; } //char(36)
        public int Row { get; set; }

    }
}
