using Microsoft.Extensions.DependencyInjection;
using Ploch.CommandLine.Spectre;
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;
using Ploch.Data.SampleApp.ConsoleApp.Commands;
using Ploch.Data.SampleApp.ConsoleApp.Services;
using Ploch.Data.SampleApp.Data;

// The CLI host comes from Ploch.CommandLine.Spectre: AppBuilder wires Microsoft.Extensions.Hosting
// (configuration + dependency injection) into a Spectre.Console.Cli command app.
// See https://github.com/mrploch/ploch-commandline for the library and its documentation.
return await AppBuilder.Create(args)
                       .WithName("sampleapp")
                       .WithDescription("Demonstrates the Ploch.Data generic repository, unit of work, and EF Core helpers.")
                       .WithVersion(new Version(1, 0, 0))
                       .ConfigureServices(services =>
                                          {
                                              // One call registers the DbContext, every repository interface and IUnitOfWork.
                                              // The connection string is read from appsettings.json.
                                              services.AddDbContextWithRepositories<SampleAppDbContext>();
                                              services.AddScoped<SampleDataSeeder>();
                                          })
                       .ConfigureCommandApp(config =>
                                            {
                                                config.Settings.ApplicationName = "sampleapp";

                                                config.AddCommand<DemoCommand>("demo")
                                                      .WithDescription("Run the full guided walkthrough of every demonstrated feature.")
                                                      .WithExample("demo")
                                                      .WithExample("demo", "--author", "\"Ada Lovelace\"", "--filler", "20");

                                                config.AddCommand<SeedCommand>("seed")
                                                      .WithDescription("Create the database and populate it with sample data.")
                                                      .WithExample("seed")
                                                      .WithExample("seed", "--filler", "50", "--keep");

                                                config.AddCommand<ListArticlesCommand>("list")
                                                      .WithDescription("List articles page by page (GetPageAsync / CountAsync).")
                                                      .WithExample("list", "--page", "2", "--page-size", "5")
                                                      .WithExample("list", "--all");

                                                config.AddCommand<ShowArticleCommand>("show")
                                                      .WithDescription("Show one article with its related entities eagerly loaded.")
                                                      .WithExample("show", "1");

                                                config.AddCommand<SearchArticlesCommand>("search")
                                                      .WithDescription("Find articles whose title contains the supplied text.")
                                                      .WithExample("search", "\"Entity Framework\"");

                                                config.AddCommand<UpdateArticleCommand>("update")
                                                      .WithDescription("Rename an article and observe automatic ModifiedTime tracking.")
                                                      .WithExample("update", "1", "--title", "\"A better title\"");

                                                config.AddCommand<ListAuthorsCommand>("authors")
                                                      .WithDescription("List authors using a directly injected read-only repository.")
                                                      .WithExample("authors");
                                            })
                       .RunAsync(args);
