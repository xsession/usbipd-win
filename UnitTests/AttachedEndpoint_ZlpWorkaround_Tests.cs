// SPDX-FileCopyrightText: 2025 Frans van Dorsselaer
//
// SPDX-License-Identifier: GPL-3.0-only

using static Usbipd.Interop.UsbIp;
using static Usbipd.Interop.VBoxUsb;

namespace UnitTests;

[TestClass]
sealed class AttachedEndpointZlpWorkaroundTests
{
    [TestMethod]
    public void NeedsZeroLengthOutWorkaroundReturnsTrueForBulkOutZlp()
    {
        var basic = new UsbIpHeaderBasic
        {
            ep = 0x02,
            direction = UsbIpDir.USBIP_DIR_OUT,
        };
        var submit = new UsbIpHeaderCmdSubmit
        {
            transfer_buffer_length = 0,
        };
        Assert.IsTrue(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(UsbSupTransferType.USBSUP_TRANSFER_TYPE_BULK, basic, submit));
        Assert.IsTrue(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(UsbSupTransferType.USBSUP_TRANSFER_TYPE_INTR, basic, submit));
    }

    [TestMethod]
    public void NeedsZeroLengthOutWorkaroundReturnsFalseForOtherCases()
    {
        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(
            UsbSupTransferType.USBSUP_TRANSFER_TYPE_BULK,
            new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_IN },
            new UsbIpHeaderCmdSubmit { transfer_buffer_length = 0 }));

        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(
            UsbSupTransferType.USBSUP_TRANSFER_TYPE_BULK,
            new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_OUT },
            new UsbIpHeaderCmdSubmit { transfer_buffer_length = 1 }));

        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(
            UsbSupTransferType.USBSUP_TRANSFER_TYPE_MSG,
            new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_OUT },
            new UsbIpHeaderCmdSubmit { transfer_buffer_length = 0 }));

        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(
            UsbSupTransferType.USBSUP_TRANSFER_TYPE_ISOC,
            new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_OUT },
            new UsbIpHeaderCmdSubmit { transfer_buffer_length = 0 }));
    }
}