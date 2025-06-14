using System;

namespace HL7lite.Test.Fluent
{
    /// <summary>
    /// Test message constants for fluent API testing
    /// </summary>
    public static class TestMessages
    {
        public const string SimpleADT = "MSH|^~\\&|SendingApp|SendingFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|12345|P|2.5\r" +
                                       "PID|||123456||Doe^John^M||19800101|M\r" +
                                       "PV1||I|ICU^101^A";

        public const string MultipleSegments = "MSH|^~\\&|TestApp|TestFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|MSG001|P|2.5\r" +
                                              "PID|||123456||Smith^Jane^A||19900215|F\r" +
                                              "DG1|1|I9|250.00^Diabetes^I9\r" +
                                              "DG1|2|I9|401.9^Hypertension^I9\r" +
                                              "IN1|1|BCBS|12345|Blue Cross\r" +
                                              "IN1|2|AETNA|67890|Aetna";

        public const string RepeatingFields = "MSH|^~\\&|TestApp|TestFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|MSG002|P|2.5\r" +
                                             "PID|||ID001~ID002~ID003||Johnson^Robert^L||19851010|M";

        public const string NullValues = "MSH|^~\\&|TestApp|TestFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|MSG003|P|2.5\r" +
                                        "PID|||123456||Brown^Mary^|||\"\"|F";

        public const string EmptyMessage = "MSH|^~\\&|TestApp|TestFacility|ReceivingApp|ReceivingFacility|20230101120000||ADT^A01|MSG004|P|2.5";
    }
}