using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleApp6
{
	class Program
	{
		private static ConcurrentDictionary<Guid, HyperGuestConnection> _clients;
		private static Dictionary<Guid, string> clientMap;

		static void Main(string[] args)
		{
			Guid serviceGuid = new Guid("{B17A9D47-5580-48F4-9E64-99AF86C8C9EE}");

			// are we running as a hypverv guest with integration services enabled?
			Guid clientVmkID = Guid.Empty;
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Virtual Machine\Guest\Parameters", false))
			{
				if (key != null)
				{
					string value = (string)key.GetValue("VirtualMachineId");
					if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out var candidate))
					{
						clientVmkID = candidate;
					}
				}
			}

			if (clientVmkID == Guid.Empty)
			{
				// assuming we're the server do we have the service we need registered?

				bool integrationServiceRegistered = false;
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Virtualization\GuestCommunicationServices", false))
				{
					if (key != null)
					{
						string[] subKeys = key.GetSubKeyNames();
						if (subKeys != null && subKeys.Length > 0)
						{
							foreach (string subKey in subKeys)
							{
								if (!string.IsNullOrEmpty(subKey) && Guid.TryParse(subKey, out Guid candidateServiceGuid))
								{
									if (candidateServiceGuid == serviceGuid)
									{
										integrationServiceRegistered = true;
										break;
									}
								}
							}
						}
					}
				}

				if (!integrationServiceRegistered)
				{
					Console.WriteLine("Error: integration service is not registered");
					return;
				}

				_clients = new ConcurrentDictionary<Guid, HyperGuestConnection>();
				ManagementObjectSearcher searcher = new ManagementObjectSearcher(
					@"root\virtualization\v2",
					@"select Name,ElementName from Msvm_ComputerSystem where description <>""Microsoft Hosting Computer System"""
				);
				clientMap = new Dictionary<Guid, string>();
				foreach (var result in searcher.Get())
				{
					if (Guid.TryParse(result.Properties["Name"]?.Value as string, out Guid vmid))
					{
						clientMap.Add(vmid, (string)result.Properties["ElementName"].Value);
					}
				}

				EndPoint endPoint = new HyperVEndPoint(HyperV.Children, serviceGuid);
				using (Socket socket = new Socket(HyperV.HyperVAddressFamily, SocketType.Stream, HyperV.HyperVProtocolType))
				{
					socket.Bind(endPoint);
					socket.Listen(1);

					Console.WriteLine("server waiting for connections");
					HyperVHost host = new HyperVHost();
					while (true)
					{
						Socket clientSocket = socket.Accept();
						HyperGuestConnection client = new HyperGuestConnection(clientSocket, host);
						client.Connected += Client_Connected;
						client.Disconnecting += Client_Disconnecting;
						client.Start();
					}
				}
			}

			else
			{
				EndPoint endPoint = new HyperVEndPoint(HyperV.Parent, serviceGuid);
				Socket socket = new Socket(HyperV.HyperVAddressFamily, SocketType.Stream, HyperV.HyperVProtocolType);
				socket.Connect(endPoint);
				HyperVGuest guest = new HyperVGuest(clientVmkID);
				guest.GuestEvent += (sender, e) => { Console.WriteLine($"guest event from host"); };
				HyperVHostConnection hostConnection = new HyperVHostConnection(socket, guest);
				hostConnection.Start();
				Console.WriteLine($"connected to host as {clientVmkID}");
				string line = null;
				while (!string.IsNullOrEmpty(line = Console.ReadLine()))
				{
					if (string.Equals(line, "event", StringComparison.InvariantCultureIgnoreCase))
					{
						guest.FireGuestEvent();
					}
				}
				hostConnection.Stop();
				Console.WriteLine("stopped");
				Console.ReadLine();
			}
		}

		private static void Client_Disconnecting(object sender, EventArgs e)
		{
			HyperGuestConnection client = (HyperGuestConnection)sender;
			_clients.Remove(client.ID, out _);
			client.Connected -= Client_Connected;
			client.Disconnected -= Client_Disconnecting;
			client.Remote.GuestEvent -= client.Local.GuestEventHandler;
			Console.WriteLine($"client {client.ID} disconnected, friendlyname is {client.Name}");
		}

		private static void Client_Connected(object sender, EventArgs e)
		{
			HyperGuestConnection client = (HyperGuestConnection)sender;
			client.Remote.GuestEvent += client.Local.GuestEventHandler;
			if (clientMap.TryGetValue(client.ID, out string name))
			{
				client.Name = name;
				Console.WriteLine($"client {client.ID} connected, friendlyname is {client.Name}");
			}
		}
	}
}
