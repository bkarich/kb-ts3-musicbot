using System;
//using System.Data.SqlClient;
//using MySql.Data.MySqlClient;
using TS3AudioBot;
using TS3AudioBot.Audio;
using TS3AudioBot.CommandSystem;
using TS3AudioBot.Plugins;
using TSLib.Full.Book;
using TSLib;
using TSLib.Full;
using LiteDB;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RankingSystem
{
	public class DataBaseKB : IBotPlugin
	{
		private TsFullClient tsFullClient;
		private PlayManager playManager;
		private Ts3Client ts3Client;
		private Connection serverView;

		private readonly List<uint> excludedGroups = new List<uint> { 11, 47, 115 }; // replace with the IDs of the excluded groups
		private readonly int UpdateInterval = 2; //min
		private Dictionary<string, User> _users;
		private List<ServerGroupInfo> _serverGroupList;

		public DataBaseKB(PlayManager playManager, Ts3Client ts3Client, Connection serverView, TsFullClient tsFull)
		{
			this.playManager = playManager;
			this.ts3Client = ts3Client;
			this.tsFullClient = tsFull;
			this.serverView = serverView;
			_users = new Dictionary<string, User>();

			_serverGroupList = new List<ServerGroupInfo>
			{
				// Year 1
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromMinutes(30), ServerGroup = (ServerGroupId)23 },//Frischling
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromHours(1), ServerGroup = (ServerGroupId)24 },//Halbe stunde 
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromHours(2), ServerGroup = (ServerGroupId)25 },//Eine Stunde
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromHours(5), ServerGroup = (ServerGroupId)26 },//2 Stunden 
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromHours(10), ServerGroup = (ServerGroupId)27 },//5 StundenNo password login
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(1), ServerGroup = (ServerGroupId)28 },//10 Stunden
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(2), ServerGroup = (ServerGroupId)29 },//1 Tag
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(4), ServerGroup = (ServerGroupId)30 },//2 Tage
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(7), ServerGroup = (ServerGroupId)31 },//4 Tage
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(15), ServerGroup = (ServerGroupId)32 },//7 Tage
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(30), ServerGroup = (ServerGroupId)33 },//15 Tage
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(60), ServerGroup = (ServerGroupId)35 },//1 Monat
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(90), ServerGroup = (ServerGroupId)36 },//2 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(120), ServerGroup = (ServerGroupId)37 },//3 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(150), ServerGroup = (ServerGroupId)38 },//4 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(180), ServerGroup = (ServerGroupId)39 },//5 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(210), ServerGroup = (ServerGroupId)40 },//6 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(240), ServerGroup = (ServerGroupId)41 },//7 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(270), ServerGroup = (ServerGroupId)42 },//8 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(300), ServerGroup = (ServerGroupId)43 },//9 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(330), ServerGroup = (ServerGroupId)44 },//10 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(365), ServerGroup = (ServerGroupId)45 },//11 Monate
				// Year 2
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(395), ServerGroup = (ServerGroupId)46 },//12 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(425), ServerGroup = (ServerGroupId)93 },//13 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(455), ServerGroup = (ServerGroupId)94 },//14 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(485), ServerGroup = (ServerGroupId)95 },//15 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(515), ServerGroup = (ServerGroupId)96 },//16 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(545), ServerGroup = (ServerGroupId)97 },//17 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(575), ServerGroup = (ServerGroupId)98 },//18 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(605), ServerGroup = (ServerGroupId)99 },//19 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(635), ServerGroup = (ServerGroupId)100 },//20 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(665), ServerGroup = (ServerGroupId)101 },//21 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(695), ServerGroup = (ServerGroupId)102 },//22 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(730), ServerGroup = (ServerGroupId)116 },//23 Monate
				// Year 3
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(395), ServerGroup = (ServerGroupId)117 },//12 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(425), ServerGroup = (ServerGroupId)118 },//13 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(455), ServerGroup = (ServerGroupId)119 },//14 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(485), ServerGroup = (ServerGroupId)120 },//15 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(515), ServerGroup = (ServerGroupId)121 },//16 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(545), ServerGroup = (ServerGroupId)122 },//17 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(575), ServerGroup = (ServerGroupId)123 },//18 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(605), ServerGroup = (ServerGroupId)124 },//19 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(635), ServerGroup = (ServerGroupId)125 },//20 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(665), ServerGroup = (ServerGroupId)126 },//21 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(695), ServerGroup = (ServerGroupId)127 },//22 Monate
				new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(730), ServerGroup = (ServerGroupId)128 },//23 Monate
				//new ServerGroupInfo { OnlineTimeThreshold = TimeSpan.FromDays(755), ServerGroup = (ServerGroupId)129 },//24 Monate
			};
		}

		public void Initialize()
		{
			//readAndLoadJson();
			InitDB();
			StartLoop();
		}

		[Command("rank")]
		public static async Task<string> CommandRank(ClientCall invoker, string querystring)
		{			
			// Retrieve the user's data from the database
			var db = new LiteDatabase("rank_users.db");
			var usersCollection = db.GetCollection<User>("users");
			
			var user = usersCollection.FindOne(x => x.UserID == invoker.ClientUid.ToString());
			Console.WriteLine("Invoker: " + invoker.NickName + " UserInDB: "+ user);

			if (user != null)
			{
				// Update the necessary fields
				TimeSpan timeSpan = TimeSpan.Parse(querystring);
				user.OnlineTime = timeSpan;
				user.LastUpdate = DateTime.Now;
				user.UpdateTime = true;

				// Update the user's data in the database
				usersCollection.Update(user);
				return $"User {user.Name} changed. New time: {user.OnlineTime.TotalDays} Days, {user.OnlineTime.Hours} hours and {user.OnlineTime.Minutes} minutes" ;
			}
			else
			{
				return "User not found.";
			}
		}

		[Command("rank")]
		public static async Task<string> CommandRank(ClientCall invoker, string DBuser, string query)
		{
			// Retrieve the user's data from the database
			var db = new LiteDatabase("rank_users.db");
			var usersCollection = db.GetCollection<User>("users");

			var user = usersCollection.FindOne(x => x.Name == DBuser);
			Console.WriteLine("Invoker: " + invoker.NickName + " UserInDB: " + user);

			if (user != null)
			{
				// Update the necessary fields
				TimeSpan timeSpan = TimeSpan.Parse(query);
				user.OnlineTime = timeSpan;
				user.LastUpdate = DateTime.Now;
				user.UpdateTime = true;

				// Update the user's data in the database
				usersCollection.Update(user);

				return $"User {user.Name} changed. New time: {user.OnlineTime.TotalDays} Days, {user.OnlineTime.Hours} hours and {user.OnlineTime.Minutes} minutes";
			}
			else
			{
				return "User not found.";
			}
			//return "Nichts gefunden!";
			//}
		}

		[Command("rankdelete")]
		public static async Task<string> CommandRankDelete(ClientCall invoker)
		{
			// Retrieve the user's data from the database
			var db = new LiteDatabase("rank_users.db");
			var usersCollection = db.GetCollection<User>("users");
			//usersCollection.Delete(Query.All());

			return "Database Cleaned";
			//}
		}

		public void readAndLoadJson()
		{
			string jsonFilePath = "tunausers.json"; // replace with your file path
			string databaseFolderPath = "oldusers.db"; // replace with your database folder path

			var users = ReadUsersFromJson(jsonFilePath);

			using (var db = new LiteDatabase(databaseFolderPath))
			{
				var usersCollection = db.GetCollection<User>("users");

				foreach (var user in users)
				{
					Console.WriteLine("User: "+ user.Nickname+" "+user.UserID);
					usersCollection.Insert(user);
				}
			}
		}

		static List<User> ReadUsersFromJson(string jsonFilePath)
		{
			var json = System.IO.File.ReadAllText(jsonFilePath);
			var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(json);

			var users = new List<User>();

			foreach (var userJson in jsonObject.data)
			{
				var user = new User
				{
					Id = userJson.uid,
					UserID = userJson.uid,
					Name = userJson.name,
					Nickname = userJson.name,
					Time = userJson.time,
					OnlineTime = TimeSpan.FromSeconds(userJson.time),
					IsAfk = false,
					IsAlone = false,
					LastUpdate = DateTime.UtcNow,
					RankGroupInt = 0,
					UpdateTime = false,
				};
				users.Add(user);

				// check for duplicate keys
				var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
				if (existingUser != null)
				{
					// update the existing user with the entry that has the highest time value
					if (user.Time > existingUser.Time)
					{
						users.Remove(existingUser);
						users.Add(user);
					}
				}
				else
				{
					users.Add(user);
				}
			}


			return users;
		}

		public ServerGroupId GetServerGroup(TimeSpan onlineTime)
		{
			foreach (var serverGroupInfo in _serverGroupList)
			{
				if (onlineTime < serverGroupInfo.OnlineTimeThreshold)
				{
					return serverGroupInfo.ServerGroup;
				}
			}

			// If the online time exceeds all thresholds, return the last server group in the list
			Console.WriteLine("Online Time Exceedet");
			return _serverGroupList.Last().ServerGroup;
		}

		private async void StartLoop()
		{
			int update = UpdateInterval;
			while (true)
			{
				Console.WriteLine($"Tick: Update:{update}");
				if (update <= 0)
				{
					// Timer end
					InitDB();
					update = UpdateInterval;
				}

				update--;
				await Task.Delay(60000); // 60000 1 min
			}
		}

		private async void InitDB()
		{
			Console.WriteLine("Updating Users");
			var allUsers = await tsFullClient.ClientList();

			try
			{
				// Get a collection (or create it if it doesn't exist)
				var db = new LiteDatabase("rank_users.db");
				var DBusers = db.GetCollection<User>("users");

				foreach (var user in allUsers.Value)
				{
					var fulluser = await tsFullClient.ClientInfo(user.ClientId);
					bool skipClient = false;

					if (fulluser.Value.ClientType.Equals(ClientType.Full))
					{
						foreach (var sg in excludedGroups)
						{
							ServerGroupId newSG = (ServerGroupId)sg;
							if (fulluser.Value.ServerGroups.Contains(newSG))
							{
								skipClient = true;
								break;
							}
						}

						if (skipClient)
						{
							continue;
						}

						//Console.WriteLine("*********** USER ***********");
						//Console.WriteLine("Valid Client");


						// Check if the user is already being tracked
						if (!_users.ContainsKey(fulluser.Value.Uid.ToString()))
						{
							// Not Yet tracked User
							var existingUser = DBusers.FindOne(u => u.UserID == fulluser.Value.Uid.ToString());
							if (existingUser != null)
							{
								// Update the existing user object with the latest information
								existingUser.Name = fulluser.Value.Name;
								existingUser.Nickname = fulluser.Value.Name;
								existingUser.LastUpdate = DateTime.Now;

								DBusers.Update(existingUser);
								_users.Add(existingUser.UserID, existingUser);
								//Console.WriteLine("Existing user loadet: " + existingUser.Name);
							}
							else
							{
								// Create a new User object for the user
								var NewUser = new User
								{
									Id = fulluser.Value.Uid.ToString(),
									Name = fulluser.Value.Name,
									UserID = fulluser.Value.Uid.ToString(),
									Time = 0,
									Nickname = fulluser.Value.Name,
									OnlineTime = TimeSpan.Zero,
									IsAfk = false,
									IsAlone = false,
									LastUpdate = DateTime.Now,
									RankGroup = (ServerGroupId)23,
									RankGroupInt= 23
								};
								var newId = await tsFullClient.GetClientDbIdFromUid((Uid)NewUser.UserID);
								await tsFullClient.ServerGroupAddClient(GetServerGroup(NewUser.OnlineTime), newId.Value.ClientDbId);
								_users.Add(NewUser.UserID, NewUser);
								// Add to DB
								DBusers.Insert(NewUser);
								//Console.WriteLine("New User Created: " + NewUser.Name);
							}
						}

						//Console.WriteLine("Update AFK");
						// Update the user's online time and AFK/alone status
						var UpdateUser = _users[fulluser.Value.Uid.ToString()];

						// If Channel has more that 1 user
						if (GetUserCountFromChannelId(fulluser.Value.ChannelId) > 1)
						{
							UpdateUser.IsAlone = false;
							//Console.WriteLine("User is NOT Alone in Channel");
						}
						else
						{
							UpdateUser.IsAlone = true;
							//Console.WriteLine("User is Alone in Channel");
						}

						// If user is AFK or Span Channel
						if (fulluser.Value.ChannelId == (ChannelId)18 || fulluser.Value.ChannelId == (ChannelId)1)
						{
							UpdateUser.IsAfk = true;
							//Console.WriteLine("User is AFK");
						}
						else
						{
							UpdateUser.IsAfk = false;
							//Console.WriteLine("User is NOT AFK");
						}
						var usrStatus = DBusers.FindOne(u => u.UserID == fulluser.Value.Uid.ToString());
						// Check if the user is in the AFK channel or alone in their channel and skip
						if (!UpdateUser.IsAfk && !UpdateUser.IsAlone)
						{
							if (usrStatus.UpdateTime)
							{
								UpdateUser.OnlineTime = usrStatus.OnlineTime;
								UpdateUser.UpdateTime = false;
								await ts3Client.SendServerMessage("[b][color=red]"+UpdateUser.Name+" online time changed! "+ usrStatus.OnlineTime.TotalDays + " Days, "+ usrStatus.OnlineTime.Hours + " hours and "+ usrStatus.OnlineTime.Minutes + " minutes[/color][/b]");
							}
							else
							{
								// Update the user's time in DB
								UpdateUser.OnlineTime += (DateTime.Now - UpdateUser.LastUpdate);
							}
							// Update the user's online time with the time since the last update

						

							// Get current Server Group check if user already has grroup addet

							//Console.WriteLine("DB Group: " + UpdateUser.RankGroupInt+" Need group: "+ GetServerGroup(UpdateUser.OnlineTime).Value.ToString());

							//UpdateUser.RankGroupInt = GetServerGroup(UpdateUser.OnlineTime).Value;

							var response = await tsFullClient.GetClientDbIdFromUid((Uid)UpdateUser.UserID);
							if (response.Ok)
							{
								var userGroups = await tsFullClient.ServerGroupsByClientDbId(response.Value.ClientDbId);
								bool hasGroup = false;
								var newId = response.Value;

								foreach (var serverGroupInfo in _serverGroupList)
								{
									if (UpdateUser.RankGroupInt == serverGroupInfo.ServerGroup.Value) { continue; }
									hasGroup = userGroups.Value.Any(g => g.ServerGroupId == serverGroupInfo.ServerGroup);
									if (hasGroup)
									{
										//Console.WriteLine("Group removed");
										// if user has Old Group Remove it
										await tsFullClient.ServerGroupDelClient(serverGroupInfo.ServerGroup, newId.ClientDbId);
									}
								}

								if (UpdateUser.RankGroupInt != GetServerGroup(UpdateUser.OnlineTime).Value)
								{								
									UpdateUser.RankGroupInt = GetServerGroup(UpdateUser.OnlineTime).Value;

									foreach (var serverGroupInfo in _serverGroupList)
									{
										//if (UpdateUser.RankGroupInt == serverGroupInfo.ServerGroup.Value) { continue; }
										hasGroup = userGroups.Value.Any(g => g.ServerGroupId == serverGroupInfo.ServerGroup);
										if (hasGroup)
										{
											//Console.WriteLine("Group removed");
											// if user has Old Group Remove it
											await tsFullClient.ServerGroupDelClient(serverGroupInfo.ServerGroup, newId.ClientDbId);
										}
									}
									//Console.WriteLine("Server Group addet");
									await tsFullClient.ServerGroupAddClient(GetServerGroup(UpdateUser.OnlineTime), newId.ClientDbId);
								}
								//Check if user has group attached
								bool hasRightGroup = userGroups.Value.Any(g => g.ServerGroupId == GetServerGroup(UpdateUser.OnlineTime));
								if (hasRightGroup)
								{
									//Console.WriteLine("User has right group attached "+ GetServerGroup(UpdateUser.OnlineTime));
								}
								else
								{
									//Console.WriteLine("Reattaching group "+ GetServerGroup(UpdateUser.OnlineTime));
									await tsFullClient.ServerGroupAddClient(GetServerGroup(UpdateUser.OnlineTime), newId.ClientDbId);
								}

							}
							else
							{
								Console.WriteLine("Update Server Group Failed");
							}

							// Update the user's last update time
							UpdateUser.LastUpdate = DateTime.Now;

							//Console.WriteLine(fulluser.Value.Name + " Online Time: " + Math.Round(UpdateUser.OnlineTime.TotalDays)+" Tage");

							// Save the user's data to the database
							DBusers.Update(UpdateUser);
							//Console.WriteLine("Database updated!");

						}
						else
						{
							//Console.WriteLine("Ignoring AFK or Alone User");
						}
						//Console.WriteLine("*********** USER ***********");

					}

				}

				// Remove any user from _users that was not found in allUsers
				var usersToRemove = new List<string>();
				foreach (var user in _users.Values)
				{
					bool deleteClient = true;
					foreach (var TSuser in allUsers.Value)
					{
						var fulluser = await tsFullClient.ClientInfo(TSuser.ClientId);

						if (user.Id == fulluser.Value.Uid.ToString())
						{
							if (user.IsAfk || user.IsAlone)
							{
								deleteClient = true;
								//Console.WriteLine(user.Name + " User AFK Continue");
								continue;

							}
							else
							{
								//Console.WriteLine(user.Name + " User found Continue");
								deleteClient = false;
								break;
							}
						}

					}
					if (deleteClient)
					{
						usersToRemove.Add(user.UserID);
						//_users.Remove(user.UserID);
						
					}
				}
				foreach (var userId in usersToRemove)
				{
					_users.Remove(userId);
					//Console.WriteLine(userId + " deleted from _user");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

		}

		private int GetUserCountFromChannelId(ChannelId ChanId)
		{
			int count = 0;

			var allConnectedClients = serverView.Clients;

			foreach (var client in allConnectedClients)
			{
				ServerGroupId serverGroupId = (ServerGroupId)11;
				if (client.Value.ServerGroups.Contains(serverGroupId))
				{
					//Console.WriteLine("User is Bot: " + client.Value.Name);
				}
				else
				{
					if (client.Value.Channel == ChanId)
					{
						count++;
					}
				}

				//Console.WriteLine(client.Value.Id + " is in channel: " + client.Value.Channel.Value.ToString());
			}

			return count;
		}

		private async void TestDB()
		{
			var allUsers = await tsFullClient.ClientList();

			try
			{
				// Replace "your_database_name" with the name of your own database file
				using (var db = new LiteDatabase("rank_users.db"))
				{
					// Get a collection (or create it if it doesn't exist)
					var users = db.GetCollection<User>("users");
					//users.Delete(Query.All());

					foreach (var user in allUsers.Value)
					{
						var fulluser = await tsFullClient.ClientInfo(user.ClientId);
						bool skipClient = false;

						if (fulluser.Value.ClientType.Equals(ClientType.Full))
						{
							foreach (var sg in excludedGroups)
							{
								ServerGroupId newSG = (ServerGroupId)sg;
								if (fulluser.Value.ServerGroups.Contains(newSG))
								{
									//Console.WriteLine(user.Name + " Online but is a Bot");
									skipClient = true;
									break;
								}
							}

							if (skipClient)
							{
								continue;
							}

							var clientOnlineTime = await tsFullClient.GetClientConnectionInfo(user.ClientId);
							// Get the client uptime (in seconds) for the user
							TimeSpan conTime = clientOnlineTime.Value.ConnectedTime;
							//ulong uptime = ts3Client.GetClientUptime(clientId).Value;
							//Console.WriteLine("Connection Time: "+ conTime);

							var adduser = new User { Name = user.Name, UserID = user.Uid?.ToString(), Time = 0 };
							//users.Insert(adduser);

						}

					}

					// Query the data
					var allUser = users.FindAll();
					//var results = users.Find(x => x.Time > 27);

					// Display the results
					foreach (var result in allUser)
					{
						Console.WriteLine($"Name: {result.Name}, USERID: {result.UserID} Time: {result.Time}");
						//Console.WriteLine(result);
					}

				}

			}catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			
		}

		public void Dispose()
		{

		}

		class RootObject
		{
			public long time { get; set; }
			public string type { get; set; }
			public string description { get; set; }
			public List<UserJson> data { get; set; }
		}

		class UserJson
		{
			public string name { get; set; }
			public string uid { get; set; }
			public int time { get; set; }
		}
	}
	public class User
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string UserID { get; set; }
		public int Time { get; set; }
		public string Nickname { get; set; }
		public TimeSpan OnlineTime { get; set; }
		public bool IsAfk { get; set; }
		public bool IsAlone { get; set; }
		public DateTime LastUpdate { get; set; }
		public ServerGroupId RankGroup { get; set; }
		public ulong RankGroupInt { get; set; }
		public bool UpdateTime { get; set; }
	}

	public class ServerGroupInfo
	{
		public TimeSpan OnlineTimeThreshold { get; set; }
		public ServerGroupId ServerGroup { get; set; }
	}
}
