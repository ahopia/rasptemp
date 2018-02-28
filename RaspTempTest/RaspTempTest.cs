using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static RaspTemp;

	[TestClass()]
	public  class TestRaspTemp
	{
		[TestMethod()]
		public  void TestLaskeKeskiArvo127()
		{
			Assert.AreEqual( 10.0, RaspTemp.LaskeKeskiArvo(new List<double>() {5.0, 15.0 }) , 0.000001, "in method LaskeKeskiArvo, line 128");
		}
		[TestMethod()]
		public  void TestLaskeMaxArvo149()
		{
			Assert.AreEqual( Double.MinValue, RaspTemp.LaskeMaxArvo(new List<double>() {}) , 0.000001, "in method LaskeMaxArvo, line 150");
			Assert.AreEqual( 25.0, RaspTemp.LaskeMaxArvo(new List<double>() {1.0, 20.0, 25.0}) , 0.000001, "in method LaskeMaxArvo, line 151");
			Assert.AreEqual( 56.0, RaspTemp.LaskeMaxArvo(new List<double>() {23.0, 56.0, 12.0}) , 0.000001, "in method LaskeMaxArvo, line 152");
			Assert.AreEqual( 14.0, RaspTemp.LaskeMaxArvo(new List<double>() {14.0, 13.0, 8.0}) , 0.000001, "in method LaskeMaxArvo, line 153");
			Assert.AreEqual( 7.0, RaspTemp.LaskeMaxArvo(new List<double>() {3.0, 7.0, 1.8}) , 0.000001, "in method LaskeMaxArvo, line 154");
		}
		[TestMethod()]
		public  void TestLaskeMinArvo174()
		{
			Assert.AreEqual( Double.MaxValue, RaspTemp.LaskeMinArvo(new List<double>() {}) , 0.000001, "in method LaskeMinArvo, line 175");
			Assert.AreEqual( 1.0, RaspTemp.LaskeMinArvo(new List<double>() {1.0, 20.0, 25.0}) , 0.000001, "in method LaskeMinArvo, line 176");
			Assert.AreEqual( 12.0, RaspTemp.LaskeMinArvo(new List<double>() {23.0, 56.0, 12.0}) , 0.000001, "in method LaskeMinArvo, line 177");
			Assert.AreEqual( 8.0, RaspTemp.LaskeMinArvo(new List<double>() {14.0, 13.0, 8.0}) , 0.000001, "in method LaskeMinArvo, line 178");
			Assert.AreEqual( 3.0, RaspTemp.LaskeMinArvo(new List<double>() {3.0, 7.0}) , 0.000001, "in method LaskeMinArvo, line 179");
			Assert.AreEqual( 3.0, RaspTemp.LaskeMinArvo(new List<double>() {3.0}) , 0.000001, "in method LaskeMinArvo, line 180");
		}
	}

