using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.PdfFonts;
using Xunit;
using static Camelot.Core;

namespace Camelot.Tests
{
    public class TextEdgesTests
    {
        [Fact]
        public void GetXCoord()
        {
            var fd = new FontDetails(string.Empty, false, 1, false);

            Assert.Throws<ArgumentOutOfRangeException>(() => TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.5550679849999f, 717.8702f }, string.Empty), "left1"));
            Assert.Throws<ArgumentOutOfRangeException>(() => TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.5550679849999f, 717.8702f }, string.Empty), "RIGHT"));

            Assert.Equal(72.0, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.5550679849999f, 717.8702f }, string.Empty), "left"), 3);
            Assert.Equal(527.5550679849999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.5550679849999f, 717.8702f }, string.Empty), "right"), 3);
            Assert.Equal(299.77753399249997, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.5550679849999f, 717.8702f }, string.Empty), "middle"), 3);
            Assert.Equal(93.59997999999996, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 93.59997999999996f, 685.6502999999999f, 229.40997999999993f, 700.6502999999999f }, string.Empty), "left"), 3);
            Assert.Equal(229.40997999999993, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 93.59997999999996f, 685.6502999999999f, 229.40997999999993f, 700.6502999999999f }, string.Empty), "right"), 3);
            Assert.Equal(161.50497999999993, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 93.59997999999996f, 685.6502999999999f, 229.40997999999993f, 700.6502999999999f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99997999999994, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99997999999994f, 660.0346999999999f, 90.34771999999994f, 671.0147f }, string.Empty), "left"), 3);
            Assert.Equal(90.34771999999994, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99997999999994f, 660.0346999999999f, 90.34771999999994f, 671.0147f }, string.Empty), "right"), 3);
            Assert.Equal(81.17384999999993, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99997999999994f, 660.0346999999999f, 90.34771999999994f, 671.0147f }, string.Empty), "middle"), 3);
            Assert.Equal(100.79997999999993, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 100.79997999999993f, 659.8204999999999f, 392.5128999999999f, 671.8204999999999f }, string.Empty), "left"), 3);
            Assert.Equal(392.5128999999999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 100.79997999999993f, 659.8204999999999f, 392.5128999999999f, 671.8204999999999f }, string.Empty), "right"), 3);
            Assert.Equal(246.65643999999992, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 100.79997999999993f, 659.8204999999999f, 392.5128999999999f, 671.8204999999999f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99545999999992, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99545999999992f, 641.1378f, 99.52249999999992f, 652.1178f }, string.Empty), "left"), 3);
            Assert.Equal(99.52249999999992, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99545999999992f, 641.1378f, 99.52249999999992f, 652.1178f }, string.Empty), "right"), 3);
            Assert.Equal(85.75897999999992, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99545999999992f, 641.1378f, 99.52249999999992f, 652.1178f }, string.Empty), "middle"), 3);
            Assert.Equal(107.99895999999993, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 107.99895999999993f, 641.1378f, 401.3076499999999f, 652.1178f }, string.Empty), "left"), 3);
            Assert.Equal(401.3076499999999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 107.99895999999993f, 641.1378f, 401.3076499999999f, 652.1178f }, string.Empty), "right"), 3);
            Assert.Equal(254.6533049999999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 107.99895999999993f, 641.1378f, 401.3076499999999f, 652.1178f }, string.Empty), "middle"), 3);
            Assert.Equal(72.00020999999992, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00020999999992f, 621.4201999999999f, 522.696463168f, 633.4201999999999f }, string.Empty), "left"), 3);
            Assert.Equal(522.696463168, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00020999999992f, 621.4201999999999f, 522.696463168f, 633.4201999999999f }, string.Empty), "right"), 3);
            Assert.Equal(297.348336584, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00020999999992f, 621.4201999999999f, 522.696463168f, 633.4201999999999f }, string.Empty), "middle"), 3);
            Assert.Equal(72.00012999999993, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00012999999993f, 607.6202f, 528.9215913119999f, 619.6202f }, string.Empty), "left"), 3);
            Assert.Equal(528.9215913119999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00012999999993f, 607.6202f, 528.9215913119999f, 619.6202f }, string.Empty), "right"), 3);
            Assert.Equal(300.4608606559999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00012999999993f, 607.6202f, 528.9215913119999f, 619.6202f }, string.Empty), "middle"), 3);
            Assert.Equal(72.00014999999996, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00014999999996f, 593.8202f, 506.539544f, 605.8202f }, string.Empty), "left"), 3);
            Assert.Equal(506.539544, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00014999999996f, 593.8202f, 506.539544f, 605.8202f }, string.Empty), "right"), 3);
            Assert.Equal(289.26984699999997, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00014999999996f, 593.8202f, 506.539544f, 605.8202f }, string.Empty), "middle"), 3);
            Assert.Equal(72.00037999999995, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00037999999995f, 580.0202f, 526.5470508200001f, 592.0202f }, string.Empty), "left"), 3);
            Assert.Equal(526.5470508200001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00037999999995f, 580.0202f, 526.5470508200001f, 592.0202f }, string.Empty), "right"), 3);
            Assert.Equal(299.27371541, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00037999999995f, 580.0202f, 526.5470508200001f, 592.0202f }, string.Empty), "middle"), 3);
            Assert.Equal(72.00037999999995, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00037999999995f, 566.2202000000001f, 529.820239408f, 578.2202000000001f }, string.Empty), "left"), 3);
            Assert.Equal(529.820239408, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00037999999995f, 566.2202000000001f, 529.820239408f, 578.2202000000001f }, string.Empty), "right"), 3);
            Assert.Equal(300.910309704, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00037999999995f, 566.2202000000001f, 529.820239408f, 578.2202000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(72.00021999999996, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00021999999996f, 552.4202000000001f, 527.4595964399999f, 564.4202000000001f }, string.Empty), "left"), 3);
            Assert.Equal(527.4595964399999, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00021999999996f, 552.4202000000001f, 527.4595964399999f, 564.4202000000001f }, string.Empty), "right"), 3);
            Assert.Equal(299.72990821999997, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 72.00021999999996f, 552.4202000000001f, 527.4595964399999f, 564.4202000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99927999999994, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99927999999994f, 538.6202000000002f, 539.94603048f, 550.6202000000002f }, string.Empty), "left"), 3);
            Assert.Equal(539.94603048, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99927999999994f, 538.6202000000002f, 539.94603048f, 550.6202000000002f }, string.Empty), "right"), 3);
            Assert.Equal(305.97265524, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99927999999994f, 538.6202000000002f, 539.94603048f, 550.6202000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99880000000002, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99880000000002f, 524.8202000000002f, 396.5388f, 536.8202000000002f }, string.Empty), "left"), 3);
            Assert.Equal(396.5388, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99880000000002f, 524.8202000000002f, 396.5388f, 536.8202000000002f }, string.Empty), "right"), 3);
            Assert.Equal(234.2688, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99880000000002f, 524.8202000000002f, 396.5388f, 536.8202000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(89.99879999999996, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99879999999996f, 505.0202000000003f, 333.53877375999997f, 517.2242000000002f }, string.Empty), "left"), 3);
            Assert.Equal(333.53877375999997, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99879999999996f, 505.0202000000003f, 333.53877375999997f, 517.2242000000002f }, string.Empty), "right"), 3);
            Assert.Equal(211.76878687999996, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99879999999996f, 505.0202000000003f, 333.53877375999997f, 517.2242000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(89.99893, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99893f, 485.22020000000026f, 353.63877f, 497.42420000000027f }, string.Empty), "left"), 3);
            Assert.Equal(353.63877, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99893f, 485.22020000000026f, 353.63877f, 497.42420000000027f }, string.Empty), "right"), 3);
            Assert.Equal(221.81885000000003, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99893f, 485.22020000000026f, 353.63877f, 497.42420000000027f }, string.Empty), "middle"), 3);
            Assert.Equal(89.99877000000004, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99877000000004f, 465.42020000000025f, 285.6896339120001f, 477.62420000000026f }, string.Empty), "left"), 3);
            Assert.Equal(285.6896339120001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99877000000004f, 465.42020000000025f, 285.6896339120001f, 477.62420000000026f }, string.Empty), "right"), 3);
            Assert.Equal(187.84420195600006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99877000000004f, 465.42020000000025f, 285.6896339120001f, 477.62420000000026f }, string.Empty), "middle"), 3);
            Assert.Equal(89.99877000000004, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99877000000004f, 445.62020000000024f, 270.6591262400001f, 457.82420000000025f }, string.Empty), "left"), 3);
            Assert.Equal(270.6591262400001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99877000000004f, 445.62020000000024f, 270.6591262400001f, 457.82420000000025f }, string.Empty), "right"), 3);
            Assert.Equal(180.32894812000006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99877000000004f, 445.62020000000024f, 270.6591262400001f, 457.82420000000025f }, string.Empty), "middle"), 3);
            Assert.Equal(89.99897000000007, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99897000000007f, 425.8202000000002f, 406.61893000000003f, 438.02420000000023f }, string.Empty), "left"), 3);
            Assert.Equal(406.61893000000003, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99897000000007f, 425.8202000000002f, 406.61893000000003f, 438.02420000000023f }, string.Empty), "right"), 3);
            Assert.Equal(248.30895000000007, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 89.99897000000007f, 425.8202000000002f, 406.61893000000003f, 438.02420000000023f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99893000000003f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99893000000003f, 400.0804000000002f, 531.6005566360001f, 412.0804000000002f }, string.Empty), "left"), 3);
            Assert.Equal(531.6005566360001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99893000000003f, 400.0804000000002f, 531.6005566360001f, 412.0804000000002f }, string.Empty), "right"), 3);
            Assert.Equal(301.7997433180001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99893000000003f, 400.0804000000002f, 531.6005566360001f, 412.0804000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99875000000009f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99875000000009f, 386.2804000000002f, 524.9551519680001f, 398.2804000000002f }, string.Empty), "left"), 3);
            Assert.Equal(524.9551519680001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99875000000009f, 386.2804000000002f, 524.9551519680001f, 398.2804000000002f }, string.Empty), "right"), 3);
            Assert.Equal(298.4769509840001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99875000000009f, 386.2804000000002f, 524.9551519680001f, 398.2804000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99865000000005f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 372.4804000000002f, 527.58299376f, 384.4804000000002f }, string.Empty), "left"), 3);
            Assert.Equal(527.58299376f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 372.4804000000002f, 527.58299376f, 384.4804000000002f }, string.Empty), "right"), 3);
            Assert.Equal(299.79082188000007, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 372.4804000000002f, 527.58299376f, 384.4804000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99865000000005f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 358.6804000000002f, 506.2175500000001f, 506.2175500000001f }, string.Empty), "left"), 3);
            Assert.Equal(506.2175500000001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 358.6804000000002f, 506.2175500000001f, 370.6804000000002f }, string.Empty), "right"), 3);
            Assert.Equal(289.10810000000004, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 358.6804000000002f, 506.2175500000001f, 370.6804000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99855000000008f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99855000000008f, 332.8804000000002f, 498.2013570600001f, 344.8804000000002f }, string.Empty), "left"), 3);
            Assert.Equal(498.2013570600001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99855000000008f, 332.8804000000002f, 498.2013570600001f, 344.8804000000002f }, string.Empty), "right"), 3);
            Assert.Equal(285.0999535300001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99855000000008f, 332.8804000000002f, 498.2013570600001f, 344.8804000000002f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99865000000011f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000011f, 319.08040000000017f, 530.5899569680001f, 331.08040000000017f }, string.Empty), "left"), 3);
            Assert.Equal(530.5899569680001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000011f, 319.08040000000017f, 530.5899569680001f, 331.08040000000017f }, string.Empty), "right"), 3);
            Assert.Equal(301.2943034840001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000011f, 319.08040000000017f, 530.5899569680001f, 331.08040000000017f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99875000000009f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99875000000009f, 305.28040000000016f, 537.907089496f, 317.28040000000016f }, string.Empty), "left"), 3);
            Assert.Equal(537.907089496f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99875000000009f, 305.28040000000016f, 537.907089496f, 317.28040000000016f }, string.Empty), "right"), 3);
            Assert.Equal(304.95291974800006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99875000000009f, 305.28040000000016f, 537.907089496f, 317.28040000000016f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99865000000005f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 291.48040000000015f, 523.79965f, 303.48040000000015f }, string.Empty), "left"), 3);
            Assert.Equal(523.79965f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 291.48040000000015f, 523.79965f, 303.48040000000015f }, string.Empty), "right"), 3);
            Assert.Equal(297.8991500000001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99865000000005f, 291.48040000000015f, 523.79965f, 303.48040000000015f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99965000000003f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99965000000003f, 277.68040000000013f, 516.334961864f, 289.68040000000013f }, string.Empty), "left"), 3);
            Assert.Equal(516.334961864f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99965000000003f, 277.68040000000013f, 516.334961864f, 289.68040000000013f }, string.Empty), "right"), 3);
            Assert.Equal(294.167305932, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99965000000003f, 277.68040000000013f, 516.334961864f, 289.68040000000013f }, string.Empty), "middle"), 3);
            Assert.Equal(71.99965000000003f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99965000000003f, 263.8804000000001f, 540.41949376f, 275.8804000000001f }, string.Empty), "left"), 3);
            Assert.Equal(540.41949376f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99965000000003f, 263.8804000000001f, 540.41949376f, 275.8804000000001f }, string.Empty), "right"), 3);
            Assert.Equal(306.20957188, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 71.99965000000003f, 263.8804000000001f, 540.41949376f, 275.8804000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(145.07965000000002f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 145.07965000000002f, 240.2960000000001f, 469.63326095968006f, 250.3160000000001f }, string.Empty), "left"), 3);
            Assert.Equal(469.63326095968006f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 145.07965000000002f, 240.2960000000001f, 469.63326095968006f, 250.3160000000001f }, string.Empty), "right"), 3);
            Assert.Equal(307.35645547984006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 145.07965000000002f, 240.2960000000001f, 469.63326095968006f, 250.3160000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(323.9270190000001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 323.9270190000001f, 220.5468000000001f, 427.81457900000004f, 230.5668000000001f }, string.Empty), "left"), 3);
            Assert.Equal(427.81457900000004f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 323.9270190000001f, 220.5468000000001f, 427.81457900000004f, 230.5668000000001f }, string.Empty), "right"), 3);
            Assert.Equal(375.87079900000003, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 323.9270190000001f, 220.5468000000001f, 427.81457900000004f, 230.5668000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(129.22863000000007f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 129.22863000000007f, 214.5546000000001f, 158.64270587028008f, 224.5746000000001f }, string.Empty), "left"), 3);
            Assert.Equal(158.64270587028008f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 129.22863000000007f, 214.5546000000001f, 158.64270587028008f, 224.5746000000001f }, string.Empty), "right"), 3);
            Assert.Equal(143.9356679351401, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 129.22863000000007f, 214.5546000000001f, 158.64270587028008f, 224.5746000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(179.80944900000006f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 179.80944900000006f, 214.5546000000001f, 192.61491921078007f, 224.5746000000001f }, string.Empty), "left"), 3);
            Assert.Equal(192.61491921078007f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 179.80944900000006f, 214.5546000000001f, 192.61491921078007f, 224.5746000000001f }, string.Empty), "right"), 3);
            Assert.Equal(186.21218410539007, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 179.80944900000006f, 214.5546000000001f, 192.61491921078007f, 224.5746000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(210.41061900000008f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 210.41061900000008f, 214.5546000000001f, 254.89504272492007f, 224.5746000000001f }, string.Empty), "left"), 3);
            Assert.Equal(254.89504272492007f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 210.41061900000008f, 214.5546000000001f, 254.89504272492007f, 224.5746000000001f }, string.Empty), "right"), 3);
            Assert.Equal(232.65283086246006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 210.41061900000008f, 214.5546000000001f, 254.89504272492007f, 224.5746000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(262.92f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 262.92f, 206.75580000000002f, 310.70575064982f, 216.7758f }, string.Empty), "left"), 3);
            Assert.Equal(310.70575064982f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 262.92f, 206.75580000000002f, 310.70575064982f, 216.7758f }, string.Empty), "right"), 3);
            Assert.Equal(286.81287532491, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 262.92f, 206.75580000000002f, 310.70575064982f, 216.7758f }, string.Empty), "middle"), 3);
            Assert.Equal(318.78f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 318.78f, 206.75580000000002f, 372.13558169705993f, 216.7758f }, string.Empty), "left"), 3);
            Assert.Equal(372.13558169705993f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 318.78f, 206.75580000000002f, 372.13558169705993f, 216.7758f }, string.Empty), "right"), 3);
            Assert.Equal(345.45779084852995, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 318.78f, 206.75580000000002f, 372.13558169705993f, 216.7758f }, string.Empty), "middle"), 3);
            Assert.Equal(380.16f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 380.16f, 206.75580000000002f, 427.39639533222f, 216.7758f }, string.Empty), "left"), 3);
            Assert.Equal(427.39639533222f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 380.16f, 206.75580000000002f, 427.39639533222f, 216.7758f }, string.Empty), "right"), 3);
            Assert.Equal(403.77819766611003, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 380.16f, 206.75580000000002f, 427.39639533222f, 216.7758f }, string.Empty), "middle"), 3);
            Assert.Equal(435.42f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 435.42f, 206.75580000000002f, 488.77558163694f, 216.7758f }, string.Empty), "left"), 3);
            Assert.Equal(488.77558163694f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 435.42f, 206.75580000000002f, 488.77558163694f, 216.7758f }, string.Empty), "right"), 3);
            Assert.Equal(462.09779081847, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 435.42f, 206.75580000000002f, 488.77558163694f, 216.7758f }, string.Empty), "middle"), 3);
            Assert.Equal(128.92784900000007f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 128.92784900000007f, 203.0316000000001f, 158.95790900000006f, 213.0516000000001f }, string.Empty), "left"), 3);
            Assert.Equal(158.95790900000006f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 128.92784900000007f, 203.0316000000001f, 158.95790900000006f, 213.0516000000001f }, string.Empty), "right"), 3);
            Assert.Equal(143.94287900000006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 128.92784900000007f, 203.0316000000001f, 158.95790900000006f, 213.0516000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(170.08991900000007f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 170.08991900000007f, 203.0316000000001f, 202.33447900000007f, 213.0516000000001f }, string.Empty), "left"), 3);
            Assert.Equal(202.33447900000007f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 170.08991900000007f, 203.0316000000001f, 202.33447900000007f, 213.0516000000001f }, string.Empty), "right"), 3);
            Assert.Equal(186.21219900000006, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 170.08991900000007f, 203.0316000000001f, 202.33447900000007f, 213.0516000000001f }, string.Empty), "middle"), 3);
            Assert.Equal(222.1137190000001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 222.1137190000001f, 203.0316000000001f, 243.1957790000001f, 213.0516000000001f }, string.Empty), "left"), 3);
            Assert.Equal(243.1957790000001f, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 222.1137190000001f, 203.0316000000001f, 243.1957790000001f, 213.0516000000001f }, string.Empty), "right"), 3);
            Assert.Equal(232.6547490000001, TextEdges.get_x_coord(TestHelper.MakeTextLine(new float[] { 222.1137190000001f, 203.0316000000001f, 243.1957790000001f, 213.0516000000001f }, string.Empty), "middle"), 3);
        }

        private readonly string[] FooTextEdgesRelevant = new string[]
        {
            "<TextEdge x=71.99 y0=641.14 y1=717.87 align=left valid=False>",
            "<TextEdge x=93.59 y0=685.65 y1=700.65 align=left valid=False>",
            "<TextEdge x=100.8 y0=659.82 y1=671.82 align=left valid=False>",
            "<TextEdge x=108 y0=641.14 y1=652.12 align=left valid=False>"
        };

        private readonly Dictionary<string, List<string>> FooTextEdges = new Dictionary<string, List<string>>()
        {
            {
                "left",
                new List<string>()
                {
                    "<TextEdge x=71.99 y0=641.14 y1=717.87 align=left valid=False>",
                    "<TextEdge x=93.59 y0=685.65 y1=700.65 align=left valid=False>",
                    "<TextEdge x=100.8 y0=659.82 y1=671.82 align=left valid=False>",
                    "<TextEdge x=108 y0=641.14 y1=652.12 align=left valid=False>"
                }
            },
            {
                "right",
                new List<string>()
                {
                    "<TextEdge x=527.55 y0=702.87 y1=717.87 align=right valid=False>",
                    "<TextEdge x=229.41 y0=685.65 y1=700.65 align=right valid=False>",
                    "<TextEdge x=90.35 y0=660.03 y1=671.01 align=right valid=False>",
                    "<TextEdge x=392.51 y0=659.82 y1=671.82 align=right valid=False>",
                    "<TextEdge x=99.52 y0=641.14 y1=652.12 align=right valid=False>",
                    "<TextEdge x=401.31 y0=641.14 y1=652.12 align=right valid=False>"
                }
            },
            {
                "middle",
                new List<string>()
                {
                    "<TextEdge x=299.78 y0=702.87 y1=717.87 align=middle valid=False>",
                    "<TextEdge x=161.5 y0=685.65 y1=700.65 align=middle valid=False>",
                    "<TextEdge x=81.17 y0=660.03 y1=671.01 align=middle valid=False>",
                    "<TextEdge x=246.66 y0=659.82 y1=671.82 align=middle valid=False>",
                    "<TextEdge x=85.76 y0=641.14 y1=652.12 align=middle valid=False>",
                    "<TextEdge x=254.65 y0=641.14 y1=652.12 align=middle valid=False>"
                }
            }
        };

        [Fact]
        public void Create()
        {
            TextEdges te0 = new TextEdges();
            Assert.Equal(50, te0.edge_tol);

            TextEdges te1 = new TextEdges(1.025f);
            Assert.Equal(1.025f, te1.edge_tol);

            TextEdges te2 = new TextEdges(3.14f);
            Assert.Equal(3.14f, te2.edge_tol);
        }

#if DEBUG
        [Fact]
        public void Generate()
        {
            var l0 = TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.555f, 717.87f }, "l0");
            var l1 = TestHelper.MakeTextLine(new float[] { 93.59f, 685.65f, 229.41f, 700.65f }, "l1");
            var l2 = TestHelper.MakeTextLine(new float[] { 71.99f, 660.03469f, 90.34772f, 671.0147f }, "l2");
            var l3 = TestHelper.MakeTextLine(new float[] { 100.799f, 659.82f, 392.513f, 671.8205f }, "l3");
            var l4 = TestHelper.MakeTextLine(new float[] { 71.995f, 641.1378f, 99.5225f, 652.1178f }, "l4");
            var l5 = TestHelper.MakeTextLine(new float[] { 107.999f, 641.1378f, 401.308f, 652.1178f }, "l5");

            // generate
            TextEdges tes = new TextEdges();
            tes.generate(new TextLine[] { l0, l1, l2, l3, l4, l5 });

            foreach (var align in new string[] { "left", "right", "middle" })
            {
                var te = tes.TextedgesForTest[align];
                var expected = FooTextEdges[align];

                Assert.Equal(expected.Count, te.Count);
                for (int t = 0; t < expected.Count; t++)
                {
                    Assert.Equal(expected[t], te[t].ToString());
                }
            }
        }
#endif

        [Fact]
        public void Find()
        {
            var l0 = TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.555f, 717.87f }, "l0");
            var l1 = TestHelper.MakeTextLine(new float[] { 93.59f, 685.65f, 229.41f, 700.65f }, "l1");
            var l2 = TestHelper.MakeTextLine(new float[] { 71.99f, 660.03469f, 90.34772f, 671.0147f }, "l2");
            var l3 = TestHelper.MakeTextLine(new float[] { 100.799f, 659.82f, 392.513f, 671.8205f }, "l3");
            var l4 = TestHelper.MakeTextLine(new float[] { 71.995f, 641.1378f, 99.5225f, 652.1178f }, "l4");
            var l5 = TestHelper.MakeTextLine(new float[] { 107.999f, 641.1378f, 401.308f, 652.1178f }, "l5");

            TextEdges tes = new TextEdges();
            tes.generate(new TextLine[] { l0, l1, l2, l3, l4, l5 });

            // find
            Assert.Equal(2, tes.find(81, "middle"));
            Assert.Equal(3, tes.find(246.2f, "middle"));
            Assert.Null(tes.find(246, "middle"));
            Assert.Equal(1, tes.find(94, "left"));
            Assert.Equal(1, tes.find(229, "right"));
            Assert.Throws<KeyNotFoundException>(() => tes.find(229, "right "));
        }

        [Fact]
        public void GetRelevant()
        {
            var l0 = TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.555f, 717.87f }, "l0");
            var l1 = TestHelper.MakeTextLine(new float[] { 93.59f, 685.65f, 229.41f, 700.65f }, "l1");
            var l2 = TestHelper.MakeTextLine(new float[] { 71.99f, 660.03469f, 90.34772f, 671.0147f }, "l2");
            var l3 = TestHelper.MakeTextLine(new float[] { 100.799f, 659.82f, 392.513f, 671.8205f }, "l3");
            var l4 = TestHelper.MakeTextLine(new float[] { 71.995f, 641.1378f, 99.5225f, 652.1178f }, "l4");
            var l5 = TestHelper.MakeTextLine(new float[] { 107.999f, 641.1378f, 401.308f, 652.1178f }, "l5");

            // generate
            TextEdges tes = new TextEdges();
            tes.generate(new TextLine[] { l0, l1, l2, l3, l4, l5 });

            var relevant = tes.get_relevant();
            Assert.Equal(FooTextEdgesRelevant.Length, relevant.Count);

            for (int r = 0; r < relevant.Count; r++)
            {
                Assert.Equal(FooTextEdgesRelevant[r], relevant[r].ToString());
            }
        }

#if DEBUG
        [Fact]
        public void GetTableAreas()
        {
            var l0 = TestHelper.MakeTextLine(new float[] { 72.0f, 702.8702f, 527.555f, 717.87f }, "l0");
            var l1 = TestHelper.MakeTextLine(new float[] { 93.59f, 685.65f, 229.41f, 700.65f }, "l1");
            var l2 = TestHelper.MakeTextLine(new float[] { 71.99f, 660.03469f, 90.34772f, 671.0147f }, "l2");
            var l3 = TestHelper.MakeTextLine(new float[] { 100.799f, 659.82f, 392.513f, 671.8205f }, "l3");
            var l4 = TestHelper.MakeTextLine(new float[] { 71.995f, 641.1378f, 99.5225f, 652.1178f }, "l4");
            var l5 = TestHelper.MakeTextLine(new float[] { 107.999f, 641.1378f, 401.308f, 652.1178f }, "l5");

            // generate
            TextEdges tes = new TextEdges();
            tes.generate(new TextLine[] { l0, l1, l2, l3, l4, l5 });

            tes.TextedgesForTest["left"][0].is_valid = true; // force one valid

            var relevant = tes.get_relevant();

            var l6 = TestHelper.MakeTextLine(new float[] { 100.799f, 650f, 392.513f, 660f }, "l6");
            var areas = tes.get_table_areas(new TextLine[] { l0, l1, l2, l3, l4, l5, l6 }, relevant);

            Assert.Single(areas);
            Assert.Equal(61.98999f, areas.Keys.First().Item1, 4);
            Assert.Equal(631.1378f, areas.Keys.First().Item2, 4);
            Assert.Equal(537.555f, areas.Keys.First().Item3, 4);
            Assert.Equal(778.54165f, areas.Keys.First().Item4, 4);
            Assert.Null(areas.Values.First());
        }
#endif
    }
}
