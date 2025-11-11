// SPDX-FileCopyrightText: 2025 Frans van Dorsselaer
//
// SPDX-License-Identifier: GPL-3.0-only

using static Usbipd.Interop.UsbIp;
using static Usbipd.Interop.VBoxUsb;

namespace UnitTests;

[TestClass]
sealed class AttachedEndpointHelperTests
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
    }

    [TestMethod]
    public void NeedsZeroLengthOutWorkaroundReturnsFalseForInOrNonBulkIntr()
    {
        var basicIn = new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_IN };
        var submitZlp = new UsbIpHeaderCmdSubmit { transfer_buffer_length = 0 };
        var submitNonZlp = new UsbIpHeaderCmdSubmit { transfer_buffer_length = 1 };

        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(UsbSupTransferType.USBSUP_TRANSFER_TYPE_BULK, basicIn, submitZlp));
        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(UsbSupTransferType.USBSUP_TRANSFER_TYPE_BULK, new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_OUT }, submitNonZlp));
        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(UsbSupTransferType.USBSUP_TRANSFER_TYPE_MSG, new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_OUT }, submitZlp));
        Assert.IsFalse(Usbipd.AttachedEndpoint.NeedsZeroLengthOutWorkaround(UsbSupTransferType.USBSUP_TRANSFER_TYPE_ISOC, new UsbIpHeaderBasic { direction = UsbIpDir.USBIP_DIR_OUT }, submitZlp));
    }
}
