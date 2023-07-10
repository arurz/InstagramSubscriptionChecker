using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using SubscriptionСheckerConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionСheckerConsoleApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("username 1:");
			var username1 = Console.ReadLine();
			Console.WriteLine("username 2:");
			var username2 = Console.ReadLine();

			string workingDirectory = Environment.CurrentDirectory;
			string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
			LoginDto loginDto = JsonConvert.DeserializeObject<LoginDto>(File.ReadAllText(projectDirectory + "\\loginSettings.json"));

			var account = new UserSessionData
			{
				UserName = loginDto.Username,
				Password = loginDto.Password
			};

			var apiclass = InstaApiBuilder.CreateBuilder()
				.SetUser(account)
				.SetRequestDelay(RequestDelay.FromSeconds(0, 1))
				.Build();

			var IsLog = await apiclass.LoginAsync();
			if (IsLog.Succeeded)
			{
				var profile1Usernames = await GetProfileFollowingsAsync(username1, apiclass);
				var profile2Usernames = await GetProfileFollowingsAsync(username2, apiclass);

				var commonFollowings = profile1Usernames
					.Intersect(profile2Usernames)
					.ToList();
				if(commonFollowings.Count == 0)
				{
					Console.WriteLine("No subscriptions matched");
				}
				else
				{
					Console.WriteLine($"Common subscriptions of {username1} and {username2}:");
					foreach (var username in commonFollowings)
					{
						Console.WriteLine(username);
					}
				}
			}
			else
			{
				Console.WriteLine("login failed");
			}
		}

		private static async Task<List<string>> GetProfileFollowingsAsync(string username, IInstaApi apiclass)
		{
			var profileSubscriptions = await apiclass.UserProcessor
					.GetUserFollowingAsync(username, PaginationParameters.MaxPagesToLoad(50));

			if (!profileSubscriptions.Succeeded)
			{
				Console.WriteLine(profileSubscriptions.Info.Message);
				throw new Exception($"{username} profile problems");
			}

			var profileUsernames = new List<string>();
			foreach (var item in profileSubscriptions.Value)
			{
				profileUsernames.Add(item.UserName);
			}			

			return profileUsernames;
		}
	}
}
