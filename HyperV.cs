using System;
using System.Net.Sockets;

namespace ConsoleApp6
{
	public static class HyperV
	{
		/// <summary>
		/// Listeners should bind to this VmId to accept connection from all partitions.
		/// HV_GUID_ZERO
		/// </summary>
		public static readonly Guid Zero = new Guid("00000000-0000-0000-0000-000000000000");

		/// <summary>
		/// Listeners should bind to this VmId to accept connection from all partitions.
		/// HV_GUID_WILDCARD
		/// </summary>
		public static readonly Guid Wildcard = new Guid("00000000-0000-0000-0000-000000000000");

		/// <summary>
		/// HV_GUID_BROADCAST
		/// </summary>
		public static readonly Guid Broadcast = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

		/// <summary>
		/// Wildcard address for children. Listeners should bind to this VmId to accept connection from its children.
		/// HV_GUID_CHILDREN
		/// </summary>
		public static readonly Guid Children = new Guid("90db8b89-0d35-4f79-8ce9-49ea0ac8b7cd");

		/// <summary>
		/// Loopback address.Using this VmId connects to the same partition as the connector.
		/// HV_GUID_LOOPBACK
		/// </summary>
		public static readonly Guid Loopback = new Guid("e0e16197-dd56-4a10-9195-5ee7a155a838");

		/// <summary>
		/// Parent address.Using this VmId connects to the parent partition of the connector.*
		/// HV_GUID_PARENT
		/// </summary>
		public static readonly Guid Parent = new Guid("a42e7cda-d03f-480c-9cc2-a4de20abb878");

		public static readonly AddressFamily HyperVAddressFamily = (AddressFamily)34;  // #define AF_HYPERV       34

		public static readonly ProtocolType HyperVProtocolType = (ProtocolType)1; // #define HV_PROTOCOL_RAW 1
	}
}
