﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Assembly.Metro.Controls.PageTemplates.Games;
using Assembly.Metro.Controls.PageTemplates.Games.Components;
using Blamite.Blam;
using Blamite.RTE.H2Vista;

namespace Assembly.Helpers.Net.Sockets
{
	public class ServerCommandHandler : IPokeCommandHandler
	{
		private NetworkPokeServer _server;
		private List<string> _clientList;
		private string _name = "Server";
	    private HaloMap _map;
		private NetworkControl.ViewModel _viewModel;

		public ServerCommandHandler(HaloMap map, NetworkControl.ViewModel viewModel)
		{
		    _map = map;
		    _viewModel = viewModel;
			_server = new NetworkPokeServer();
			HandleClientListCommand(new ClientListCommand(GetClientNameList()));
			var thread = new Thread(new ThreadStart(delegate
			{
				while (true)
				{
				    _server.ReceiveCommand(this);
				}
			}));
			thread.Start();
		}

		public void HandleFreezeCommand(FreezeCommand freeze)
		{
			HandlerFunctions.FreezeCommand(freeze);
			_server.SendCommandToAll(freeze);
		}

		public void HandleMemoryCommand(MemoryCommand memory)
		{
			HandleFreezeCommand(new FreezeCommand(true));
			_server.SendCommandToAll(memory);
			if (_map.CacheFile.Engine == EngineType.ThirdGeneration)
			{
				HandlerFunctions.MemoryCommand(memory, _map);
			}
			HandleFreezeCommand(new FreezeCommand(false));
		}

		public void StartFreezeCommand(FreezeCommand freeze)
		{
			HandleFreezeCommand(freeze);
		}

		public void StartMemoryCommand(MemoryCommand memory)
		{
			HandleMemoryCommand(memory);
		}

		public void StartNameChangeCommand(ChangeNameCommand changeNameCommand)
		{
			_name = changeNameCommand.Name;

			HandleClientListCommand(new ClientListCommand(GetClientNameList()));
		}

		public List<ClientModel> GetClientList()
		{
			return _server.GetClients();
		} 

		public List<string> GetClientNameList()
		{
			_clientList = new List<string>();
			_clientList.Add(_name);
			foreach (var client in GetClientList())
			{
				_clientList.Add(client.Name);
			}
			_viewModel.Clients = new ObservableCollection<string>(_clientList);
			return _clientList;
		}

		public void HandleChangeNameCommand(ChangeNameCommand changeNameCommand)
		{
			changeNameCommand.Source.Name = changeNameCommand.Name;
			HandleClientListCommand(new ClientListCommand(GetClientNameList()));
		}

		public void HandleClientListCommand(ClientListCommand clientListCommand)
		{
			_server.SendCommandToAll(new ClientListCommand(GetClientNameList()));
		}
	}
}
