using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp6
{
	//	typedef struct _SOCKADDR_HV
	//	{
	//		ADDRESS_FAMILY Family;
	//		USHORT Reserved;
	//		GUID VmId;
	//		GUID ServiceId;
	//	}
	//	SOCKADDR_HV, * PSOCKADDR_HV;
	[DebuggerDisplay("{ToString(),nq}")]
	public class HyperVEndPoint : EndPoint
	{
		private	const int AddressSize = 2 + 2 + 16 + 16;
		private const int VMIDOddset = 2 + 2;
		private const int ServiceIDOffset = 2 + 2 + 16;

		public HyperVEndPoint(Guid vmId, Guid serviceId)
		{
			VMID = vmId;
			ServiceID = serviceId;
		}

		public Guid VMID { get; }

		public Guid ServiceID { get; }

		public override AddressFamily AddressFamily => HyperV.HyperVAddressFamily;

		public override SocketAddress Serialize()
		{
			SocketAddress address = new SocketAddress((AddressFamily)34,AddressSize);

			// reserved, set to 0
			address[2] = 0;
			address[3] = 0;

			Span<byte> buffer = stackalloc byte[16];

			VMID.TryWriteBytes(buffer);
			for (int index = 0; index < 16; index++)
			{
				address[VMIDOddset + index] = buffer[index];
			}

			ServiceID.TryWriteBytes(buffer);
			for (int index = 0; index < 16; index++)
			{
				address[ServiceIDOffset + index] = buffer[index];
			}

			return address;
		}

		public override EndPoint Create(SocketAddress socketAddress)
		{
			if (socketAddress is null)
			{
				throw new ArgumentNullException(nameof(socketAddress));
			}
			if (socketAddress.Family != HyperV.HyperVAddressFamily)
			{
				throw new ArgumentException(nameof(AddressFamily));
			}
			if (socketAddress.Size != AddressSize)
			{
				throw new ArgumentException(nameof(socketAddress.Size));
			}

		
			Span<byte> buffer = stackalloc byte[16];
			for (int index = 0; index < 16; index++)
			{
				buffer[index] = socketAddress[VMIDOddset + index];
			}
			Guid vmId = new Guid(buffer);

			for (int index = 0; index < 16; index++)
			{
				buffer[index] = socketAddress[ServiceIDOffset + index];
			}
			return new HyperVEndPoint(vmId, new Guid(buffer));
		}

		public override bool Equals(object obj)
		{
			return (obj is HyperVEndPoint hyperVEndPoint && VMID == hyperVEndPoint.VMID && ServiceID == hyperVEndPoint.ServiceID);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(VMID, ServiceID);
		}

		public override string ToString()
		{
			return string.Create(1 + 36 + 1 + 36 + 1,
				this,
				(buffer,state) =>
				{
					buffer[0] = '[';
					state.VMID.TryFormat(buffer.Slice(1, 36),out _, "D");
					buffer[1 + 36] = '|';
					state.ServiceID.TryFormat(buffer.Slice(1 + 36 + 1, 36), out _, "D");
					buffer[1 + 36 + 1 + 36] = ']';
				}
			);
		}
	}
}
