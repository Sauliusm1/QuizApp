
using BackEnd.Data.Dtos;
using BackEnd.Data.Repositories;
using BackEnd.Strategy;
using FluentValidation;
using FluentValidation.Results;

namespace BackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            //app.UseAuthorization();


            app.MapGet("/quizQuestions", (HttpContext httpContext, IQuizQuestionRepository quizQuestionRepository) =>
            {
                var quizQuestions = quizQuestionRepository.GetQuizQuestions();
                if (quizQuestions == null || !quizQuestions.Any())
                {
                    return Results.NoContent();
                }

                return Results.Ok(quizQuestions);
            })
            .WithName("GetQuizQuestions")
            .WithOpenApi();

            app.MapPost("/quizSubmit", (HttpContext httpContext, IQuizQuestionRepository quizQuestionRepository, CreateScoreDto createScoreDto) =>
            {
                CreateScoreDtoValidator validator = new CreateScoreDtoValidator();
                ValidationResult validationResult = validator.Validate(createScoreDto);
                if (validationResult.IsValid) { 
                    var quizQuestions = quizQuestionRepository.GetQuizAnswers();
                    if (quizQuestions == null || !quizQuestions.Any())
                    {
                        return Results.NoContent();
                    }

                    ScoringContext context = new ScoringContext();
                    SimpleScoring simpleScoring = new SimpleScoring();
                    MulitipleScoring mulitipleScoring = new MulitipleScoring();
                    context.SetScoringStrategy(simpleScoring);
                    int score = 0;
                    string[] answers = createScoreDto.answers;
                    Console.WriteLine(quizQuestions.Count);
                    for (int i = 0; i < quizQuestions.Count; i++)
                    {
                        if(quizQuestions[i].Type != 1)
                        {
                            context.SetScoringStrategy(simpleScoring);
                        }
                        else
                        {
                            context.SetScoringStrategy(mulitipleScoring);
                        }
                        score += context.GetScore(answers[i], quizQuestions[i].CorrectAnswer);
                    }
                    return Results.Ok(score);
                }
                else
                {
                    //TODO: Find which error works best for invalid email
                    return Results.BadRequest();
                }
            })
            .WithName("PostQuizAnswers")
            .WithOpenApi();

            app.Run();
        }
    }
}
