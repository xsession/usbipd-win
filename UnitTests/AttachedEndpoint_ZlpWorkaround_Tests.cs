// SPDX-FileCopyrightText: 2025 Frans van Dorsselaer
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using static Usbipd.Interop.UsbIp;
using static Usbipd.Interop.VBoxUsb;

namespace UnitTests;

[TestClass]
public sealed class AttachedEndpoint_ZlpWorkaround_Tests
{
    sealed class TestDeviceFile : Usbipd.DeviceFile
    {
        public TestDeviceFile() : base("\\\\.\\NUL") { }
        public UsbSupUrb? CapturedUrb { get; private set; }
        public override Task<uint> IoControlAsync(uint ioControlCode, byte[]? input, byte[]? output, bool exactOutput = true)
        {
            if (ioControlCode == (uint)SUPUSB_IOCTL.SEND_URB && input is not null && input.Length == Unsafe.SizeOf<UsbSupUrb>())
            {
                Usbipd.Tools.BytesToStruct(input, out UsbSupUrb urb);
                CapturedUrb = urb;
            }
            return Task.FromResult(0u);
        }
    }

    [TestMethod]
    public async Task BulkOutZeroLength_AllocatesDummyBufferButKeepsLenZero()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger("test");

        var deviceFile = new TestDeviceFile();
        var clientContext = new Usbipd.ClientContext(new System.Net.Sockets.TcpClient(), deviceFile);

        var pcap = new Usbipd.PcapNg(System.IO.Stream.Null);
        var replyChannel = Channel.CreateUnbounded<Usbipd.RequestReply>();
        var endpoint = new Usbipd.AttachedEndpoint(logger, clientContext, pcap, 0x02, replyChannel, CancellationToken.None);

        var basic = new UsbIpHeaderBasic
        {
            command = UsbIpCmd.USBIP_CMD_SUBMIT,
            seqnum = 1,
            ep = 0x02, // OUT bulk endpoint address
            direction = UsbIpDir.USBIP_DIR_OUT,
        };
        var submit = new UsbIpHeaderCmdSubmit
        {
            transfer_flags = 0,
            transfer_buffer_length = 0,
            number_of_packets = -1,
        };

        await endpoint.HandleSubmitAsync(basic, submit, CancellationToken.None);

        Assert.IsNotNull(deviceFile.CapturedUrb, "URB should have been captured");
        Assert.AreEqual((uint)0, deviceFile.CapturedUrb!.len, "URB length must remain zero for true ZLP");
        // buf pointer should be non-zero (dummy buffer allocated and pinned)
        Assert.AreNotEqual(nint.Zero, deviceFile.CapturedUrb.buf, "Dummy buffer pointer should be non-zero");
    }
    // Removed: superseded by AttachedEndpoint_Helper_Tests
