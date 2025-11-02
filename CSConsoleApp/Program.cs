using System.Runtime.InteropServices;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CSConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var filePath = System.IO.Directory.GetFiles(currentDirectory, "*.csv").First();

            IReadOnlyList<MovieCredit> movieCredits = null;
            try
            {
                var parser = new MovieCreditsParser(filePath);
                movieCredits = parser.Parse(); // Тип переменной теперь IReadOnlyList<MovieCredit>
            }
            catch (Exception exc)
            {
                Console.WriteLine("Не удалось распарсить csv");
                Environment.Exit(1);
            }
            var top10Actors = movieCredits
                                .SelectMany(movie => movie.Cast) // Объединяем всех актеров из всех фильмов в одну последовательность
                                .GroupBy(castMember => castMember.Name) // Группируем по имени актера
                                .Select(group => new
                                {
                                    ActorName = group.Key,
                                    MovieCount = group.Count() // Считаем количество фильмов для каждого
                                })
                                .OrderByDescending(actor => actor.MovieCount) // Сортируем по убыванию количества фильмов
                                .Take(10); // Берем первые 10

            Console.WriteLine(string.Join(Environment.NewLine, top10Actors.Select(a => $"{a.ActorName} - {a.MovieCount}")));


        }
    }
	public static class Program
	{
		public static void Main()
		{
			var currentDirectory = System.IO.Directory.GetCurrentDirectory();
			var filePath = System.IO.Directory.GetFiles(currentDirectory, "*.csv").First();

			IReadOnlyList<MovieCredit> movieCredits = null;
			try
			{
				var parser = new MovieCreditsParser(filePath);
				movieCredits = parser.Parse(); // Тип переменной теперь IReadOnlyList<MovieCredit>
			}
			catch (Exception exc)
			{
				Console.WriteLine("Не удалось распарсить csv");
				Environment.Exit(1);
			}

			// 1) Найти все фильмы, снятые режиссером "Steven Spielberg".
			var targetDirector = "Steven Spielberg";
            var spielbergMovies = movieCredits
				.Where(m => m.Crew.Any(c => c.Job == "Director" && c.Name == targetDirector))
				.Select(m => m.Title)
				.OrderBy(t => t)
				.ToList();
			Console.WriteLine($"1) films of {targetDirector}:");
			Console.WriteLine(string.Join(Environment.NewLine, spielbergMovies));
			Console.WriteLine();

			// 2) Список всех персонажей, которых сыграл актер "Tom Hanks".
			var tomHanksCharacters = movieCredits
				.SelectMany(m => m.Cast.Select(c => (m.MovieId, c)))
				.Where(x => x.c.Name == "Tom Hanks")
				.Select(x => x.c.Character)
				.Where(ch => !string.IsNullOrWhiteSpace(ch))
				.Distinct()
				.OrderBy(ch => ch)
				.ToList();
			Console.WriteLine("2:");
			Console.WriteLine(string.Join(Environment.NewLine, tomHanksCharacters));
			Console.WriteLine();

			// 3) 5 фильмов с самым большим количеством актеров в составе.
			var top5LargestCasts = movieCredits
				.Select(m => new { m.Title, CastCount = m.Cast.Count })
				.OrderByDescending(x => x.CastCount)
				.ThenBy(x => x.Title)
				.Take(5)
				.ToList();
			Console.WriteLine("3:");
			Console.WriteLine(string.Join(Environment.NewLine, top5LargestCasts.Select(x => $"{x.Title} - {x.CastCount}")));
			Console.WriteLine();

			// 4) Топ-10 самых востребованных актеров (по количеству фильмов).
			var top10Actors = movieCredits
				.SelectMany(m => m.Cast.Select(c => new { c.Name, m.MovieId }))
				.Distinct()
				.GroupBy(x => x.Name)
				.Select(g => new { ActorName = g.Key, MovieCount = g.Select(x => x.MovieId).Distinct().Count() })
				.OrderByDescending(x => x.MovieCount)
				.ThenBy(x => x.ActorName)
				.Take(10)
				.ToList();
			Console.WriteLine("4:");
			Console.WriteLine(string.Join(Environment.NewLine, top10Actors.Select(a => $"{a.ActorName} - {a.MovieCount}")));
			Console.WriteLine();

			// 5) Уникальные департаменты съемочной группы.
			var uniqueDepartments = movieCredits
				.SelectMany(m => m.Crew)
				.Select(c => c.Department)
				.Where(d => !string.IsNullOrWhiteSpace(d))
				.Distinct()
				.OrderBy(d => d)
				.ToList();
			Console.WriteLine("5:");
			Console.WriteLine(string.Join(Environment.NewLine, uniqueDepartments));
			Console.WriteLine();

			// 6) Фильмы, где Hans Zimmer был Original Music Composer.
			var hansZimmerMovies = movieCredits
				.Where(m => m.Crew.Any(c => c.Name == "Hans Zimmer" && c.Job == "Original Music Composer"))
				.Select(m => m.Title)
				.OrderBy(t => t)
				.ToList();
			Console.WriteLine("6:");
			Console.WriteLine(string.Join(Environment.NewLine, hansZimmerMovies));
			Console.WriteLine();

			// 7) Словарь: ключ — ID фильма, значение — имя режиссера (берем первого режиссера, если их несколько).
			var movieIdToDirector = movieCredits
				.Select(m => new { m.MovieId, Director = m.Crew.FirstOrDefault(c => c.Job == "Director")?.Name })
				.Where(x => !string.IsNullOrWhiteSpace(x.Director))
				.ToDictionary(x => x.MovieId, x => x.Director!);
			Console.WriteLine("7: first10: filmId -> director:");
			foreach (var kv in movieIdToDirector.Take(10).OrderBy(kv => kv.Key))
			{
				Console.WriteLine($"{kv.Key} -> {kv.Value}");
			}
			Console.WriteLine();

			// 8) Фильмы, где в актерском составе есть и Brad Pitt, и George Clooney.
			var pitClooneyMovies = movieCredits
				.Where(m => m.Cast.Any(c => c.Name == "Brad Pitt") && m.Cast.Any(c => c.Name == "George Clooney"))
				.Select(m => m.Title)
				.OrderBy(t => t)
				.ToList();
			Console.WriteLine("8:");
			Console.WriteLine(string.Join(Environment.NewLine, pitClooneyMovies));
			Console.WriteLine();

			// 9) Сколько всего человек работает в департаменте Camera (уникальные люди по Id) по всем фильмам.
			var cameraPeopleCount = movieCredits
				.SelectMany(m => m.Crew.Where(c => c.Department == "Camera").Select(c => c.Id))
				.Distinct()
				.Count();
			Console.WriteLine($"9: {cameraPeopleCount}");
			Console.WriteLine();

			// 10) Люди, которые в фильме Titanic были одновременно и в съемочной группе, и в списке актеров.
			var titanic = movieCredits.FirstOrDefault(m => m.Title == "Titanic");
			var titanicOverlap = titanic == null
				? new List<string>()
				: titanic.Cast.Select(c => c.Name).Intersect(titanic.Crew.Select(c => c.Name)).OrderBy(n => n).ToList();
			Console.WriteLine("10:");
			Console.WriteLine(string.Join(Environment.NewLine, titanicOverlap));
			Console.WriteLine();

			// 11) "Внутренний круг" режиссера Quentin Tarantino: топ-5 членов съемочной группы (не актеров), по числу фильмов.
			var tarantinoMovies = movieCredits
				.Where(m => m.Crew.Any(c => c.Job == "Director" && c.Name == "Quentin Tarantino"))
				.ToList();
			var tarantinoInnerCircle = tarantinoMovies
				.SelectMany(m => m.Crew.Where(c => c.Job != "Director").Select(c => new { c.Id, c.Name, m.MovieId }))
				.Distinct()
				.GroupBy(x => new { x.Id, x.Name })
				.Select(g => new { g.Key.Name, MovieCount = g.Select(x => x.MovieId).Distinct().Count() })
				.OrderByDescending(x => x.MovieCount)
				.ThenBy(x => x.Name)
				.Take(5)
				.ToList();
			Console.WriteLine("11:");
			Console.WriteLine(string.Join(Environment.NewLine, tarantinoInnerCircle.Select(x => $"{x.Name} - {x.MovieCount}")));
			Console.WriteLine();

			// 12) Экранные дуэты: 10 пар актеров, которые чаще всего снимались вместе.
			var duoCounts = new Dictionary<(string A, string B), int>();
			foreach (var m in movieCredits)
			{
				var names = m.Cast.Select(c => c.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToArray();
				for (int i = 0; i < names.Length; i++)
				{
					for (int j = i + 1; j < names.Length; j++)
					{
						var key = (names[i], names[j]);
						duoCounts[key] = duoCounts.TryGetValue(key, out var cnt) ? cnt + 1 : 1;
					}
				}
			}
			var top10Duos = duoCounts
				.OrderByDescending(kv => kv.Value)
				.ThenBy(kv => kv.Key.A)
				.ThenBy(kv => kv.Key.B)
				.Take(10)
				.ToList();
			Console.WriteLine("12:");
			Console.WriteLine(string.Join(Environment.NewLine, top10Duos.Select(kv => $"{kv.Key.A} & {kv.Key.B} - {kv.Value}")));
			Console.WriteLine();

			// 13) Индекс разнообразия: топ-5 членов съемочной группы по числу уникальных департаментов.
			var diversityTop5 = movieCredits
				.SelectMany(m => m.Crew.Select(c => new { c.Id, c.Name, c.Department }))
				.Where(x => !string.IsNullOrWhiteSpace(x.Department))
				.GroupBy(x => new { x.Id, x.Name })
				.Select(g => new { g.Key.Name, DeptCount = g.Select(x => x.Department).Distinct().Count() })
				.OrderByDescending(x => x.DeptCount)
				.ThenBy(x => x.Name)
				.Take(5)
				.ToList();
			Console.WriteLine("13:");
			Console.WriteLine(string.Join(Environment.NewLine, diversityTop5.Select(x => $"{x.Name} - {x.DeptCount}")));
			Console.WriteLine();

			// 14) Творческие трио: фильмы, где один человек был Director, Writer и Producer.
			var creativeTrios = movieCredits
				.Select(m => new
				{
					m.Title,
					Trios = m.Crew
						.GroupBy(c => new { c.Id, c.Name })
						.Where(g => g.Select(x => x.Job).Distinct().Contains("Director")
							&& g.Select(x => x.Job).Distinct().Contains("Writer")
							&& g.Select(x => x.Job).Distinct().Contains("Producer"))
						.Select(g => g.Key.Name)
						.Distinct()
						.ToList()
				})
				.Where(x => x.Trios.Any())
				.ToList();
			Console.WriteLine("14:");
			foreach (var entry in creativeTrios)
			{
				Console.WriteLine($"{entry.Title}: {string.Join(", ", entry.Trios)}");
			}
			Console.WriteLine();

			// 15) Два шага до Кевина Бейкона.
			string kevin = "Kevin Bacon";
			var graph = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (var m in movieCredits)
			{
				var actors = m.Cast.Select(c => c.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToArray();
				for (int i = 0; i < actors.Length; i++)
				{
					for (int j = i + 1; j < actors.Length; j++)
					{
						if (!graph.TryGetValue(actors[i], out var setI)) { setI = new HashSet<string>(StringComparer.OrdinalIgnoreCase); graph[actors[i]] = setI; }
						if (!graph.TryGetValue(actors[j], out var setJ)) { setJ = new HashSet<string>(StringComparer.OrdinalIgnoreCase); graph[actors[j]] = setJ; }
						setI.Add(actors[j]);
						setJ.Add(actors[i]);
					}
				}
			}
			var oneStep = graph.ContainsKey(kevin) ? graph[kevin] : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var twoSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var n1 in oneStep)
			{
				if (!graph.TryGetValue(n1, out var neigh)) continue;
				foreach (var n2 in neigh)
				{
					if (!n2.Equals(kevin, StringComparison.OrdinalIgnoreCase) && !oneStep.Contains(n2))
					{
						twoSteps.Add(n2);
					}
				}
			}
			Console.WriteLine("15:");
			Console.WriteLine("Total: " + twoSteps.Count);
			Console.WriteLine(string.Join(Environment.NewLine, twoSteps.OrderBy(n => n).Take(25)));
			Console.WriteLine();

			// 16) Сгруппировать фильмы по режиссеру и найти средний размер Cast и Crew.
			var moviesWithDirector = movieCredits
				.Select(m => new { m, Director = m.Crew.FirstOrDefault(c => c.Job == "Director")?.Name })
				.Where(x => !string.IsNullOrWhiteSpace(x.Director))
				.ToList();
			var directorAverages = moviesWithDirector
				.GroupBy(x => x.Director!)
				.Select(g => new
				{
					Director = g.Key!,
					AvgCast = g.Average(x => x.m.Cast.Count),
					AvgCrew = g.Average(x => x.m.Crew.Count)
				})
				.OrderByDescending(x => x.AvgCast)
				.ToList();
			Console.WriteLine("16:\n first 10:");
			foreach (var row in directorAverages.Take(10))
			{
				Console.WriteLine($"{row.Director} - Cast: {row.AvgCast:F2}, Crew: {row.AvgCrew:F2}");
			}
			Console.WriteLine();

			// 17) Универсалы: для людей, которые и актеры, и в crew — их самый частый департамент.
			var actorIds = movieCredits.SelectMany(m => m.Cast.Select(c => c.Id)).ToHashSet();
			var crewByPerson = movieCredits
				.SelectMany(m => m.Crew.Select(c => c))
				.GroupBy(c => new { c.Id, c.Name })
				.ToDictionary(g => g.Key, g => g.ToList());
			var universals = crewByPerson
				.Where(kv => actorIds.Contains(kv.Key.Id))
				.Select(kv => new
				{
					kv.Key.Name,
					DominantDepartment = kv.Value
						.Where(c => !string.IsNullOrWhiteSpace(c.Department))
						.GroupBy(c => c.Department)
						.OrderByDescending(g => g.Count())
						.ThenBy(g => g.Key)
						.Select(g => g.Key)
						.FirstOrDefault()
				})
				.Where(x => !string.IsNullOrWhiteSpace(x.DominantDepartment))
				.OrderBy(x => x.Name)
				.Take(25)
				.ToList();
			Console.WriteLine("17:\n first 25:");
			Console.WriteLine(string.Join(Environment.NewLine, universals.Select(x => $"{x.Name} -> {x.DominantDepartment}")));
			Console.WriteLine();

			// 18) Пересечение элитных клубов: люди, работавшие и с Scorsese, и с Nolan.
			var scorseseMovies = movieCredits.Where(m => m.Crew.Any(c => c.Job == "Director" && c.Name == "Martin Scorsese")).ToList();
			var nolanMovies = movieCredits.Where(m => m.Crew.Any(c => c.Job == "Director" && c.Name == "Christopher Nolan")).ToList();
			var scorsesePeople = new HashSet<int>(scorseseMovies.SelectMany(m => m.Cast.Select(c => c.Id).Concat(m.Crew.Select(c => c.Id))));
			var nolanPeople = new HashSet<int>(nolanMovies.SelectMany(m => m.Cast.Select(c => c.Id).Concat(m.Crew.Select(c => c.Id))));
			var eliteIntersectionIds = scorsesePeople.Intersect(nolanPeople).ToHashSet();
			var idToName = movieCredits
				.SelectMany(m => m.Cast.Select(c => new { c.Id, c.Name }).Concat(m.Crew.Select(c => new { c.Id, c.Name })))
				.GroupBy(x => x.Id)
				.ToDictionary(g => g.Key, g => g.Select(x => x.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? g.First().Name);
			var eliteIntersectionNames = eliteIntersectionIds
				.Select(id => idToName.TryGetValue(id, out var name) ? name : id.ToString())
				.OrderBy(n => n)
				.Take(50)
				.ToList();
			Console.WriteLine("18:\nfirst 50:");
			Console.WriteLine(string.Join(Environment.NewLine, eliteIntersectionNames));
			Console.WriteLine();

			// 19) Скрытое влияние: департаменты по среднему количеству актеров в фильмах, где они участвовали.
			var deptInfluence = movieCredits
				.Select(m => new { m, Departments = m.Crew.Select(c => c.Department).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct() })
				.SelectMany(x => x.Departments.Select(d => new { Department = d, CastCount = x.m.Cast.Count }))
				.GroupBy(x => x.Department)
				.Select(g => new { Department = g.Key, AvgCast = g.Average(x => x.CastCount), Movies = g.Count() })
				.OrderByDescending(x => x.AvgCast)
				.ThenBy(x => x.Department)
				.ToList();
			Console.WriteLine("19:\nfirst 15:");
			foreach (var row in deptInfluence.Take(15))
			{
				Console.WriteLine($"{row.Department} - AvgCast: {row.AvgCast:F2} (films: {row.Movies})");
			}
			Console.WriteLine();

			// 20) Архетипы персонажей для Johnny Depp: группировка по первому слову роли.
			string targetActor = "Johnny Depp";
			var deppArchetypes = movieCredits
				.SelectMany(m => m.Cast.Where(c => c.Name == targetActor))
				.Select(c => (string.IsNullOrWhiteSpace(c.Character) ? null : c.Character.Trim()))
				.Where(ch => !string.IsNullOrWhiteSpace(ch))
				.Select(ch =>
				{
					var idx = ch!.IndexOf(' ');
					return idx > 0 ? ch!.Substring(0, idx) : ch!;
				})
				.GroupBy(first => first)
				.Select(g => new { Archetype = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.ThenBy(x => x.Archetype)
				.ToList();
			Console.WriteLine($"20:\n roles of {targetActor}:");
			Console.WriteLine(string.Join(Environment.NewLine, deppArchetypes.Select(x => $"{x.Archetype} - {x.Count}")));
			Console.WriteLine();
		}
	}
}
