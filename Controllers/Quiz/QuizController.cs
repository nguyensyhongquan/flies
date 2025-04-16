using FliesProject.Data;
using FliesProject.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FliesProject.Controllers
{
    public class QuizController : Controller
    {
        private readonly FiliesContext _context;

        public QuizController(FiliesContext context)
        {
            _context = context;
        }

        // GET: Quiz/Create
        public IActionResult Create()
        {
            return View(new Quiz());
        }

        // POST: Quiz/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz, List<string> answers, string isCorrects, List<string> isCorrectsMultiple, string writingSample)
        {
            if (ModelState.IsValid)
            {
                quiz.CreatedAt = DateTime.Now;
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                var question = new QuizQuestion
                {
                    QuizId = quiz.QuizId,
                    QuestionText = quiz.Content,
                    QuestionType = quiz.QuizType,
                    CreatedAt = DateTime.Now
                };
                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                if (quiz.QuizType != "Writing")
                {
                    if (quiz.QuizType == "TrueFalse")
                    {
                        var correctIndex = int.TryParse(isCorrects, out int index) ? index : 0;
                        for (int i = 0; i < answers.Count; i++)
                        {
                            var answer = new QuizAnswer
                            {
                                QuestionId = question.QuestionId,
                                AnswerText = answers[i],
                                IsCorrect = (i == correctIndex)
                            };
                            _context.QuizAnswers.Add(answer);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < answers.Count; i++)
                        {
                            var answer = new QuizAnswer
                            {
                                QuestionId = question.QuestionId,
                                AnswerText = answers[i],
                                IsCorrect = isCorrectsMultiple.Contains(i.ToString())
                            };
                            _context.QuizAnswers.Add(answer);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(writingSample))
                    {
                        var sample = new QuizWritingSample
                        {
                            QuestionId = question.QuestionId,
                            SampleAnswer = writingSample
                        };
                        _context.QuizWritingSamples.Add(sample);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Course");
            }

            // Log ModelState errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("ModelState Errors: " + string.Join(", ", errors));
            return View(quiz);
        }
    }
}