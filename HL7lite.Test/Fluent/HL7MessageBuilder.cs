using System;
using System.Collections.Generic;
using System.Text;

namespace HL7lite.Test.Fluent
{
    /// <summary>
    /// Fluent builder for creating test HL7 messages
    /// </summary>
    public class HL7MessageBuilder
    {
        private readonly List<string> _segments = new List<string>();
        private readonly HL7Encoding _encoding = new HL7Encoding();

        public static HL7MessageBuilder Create()
        {
            return new HL7MessageBuilder();
        }

        public HL7MessageBuilder WithMSH(string sendingApp = "TestApp", string messageType = "ADT^A01", string controlId = "12345")
        {
            var msh = $"MSH|^~\\&|{sendingApp}|TestFacility|ReceivingApp|ReceivingFacility|20230101120000||{messageType}|{controlId}|P|2.5";
            _segments.Add(msh);
            return this;
        }

        public HL7MessageBuilder WithPID(string patientId = "123456", string lastName = "Doe", string firstName = "John", string middleName = "", string dob = "19800101", string gender = "M")
        {
            var name = string.IsNullOrEmpty(middleName) ? $"{lastName}^{firstName}" : $"{lastName}^{firstName}^{middleName}";
            var pid = $"PID|||{patientId}||{name}||{dob}|{gender}";
            _segments.Add(pid);
            return this;
        }

        public HL7MessageBuilder WithPV1(string patientClass = "I", string location = "ICU^101^A")
        {
            var pv1 = $"PV1||{patientClass}|{location}";
            _segments.Add(pv1);
            return this;
        }

        public HL7MessageBuilder WithDG1(int setId, string diagnosisCode, string description = "")
        {
            var diagnosis = string.IsNullOrEmpty(description) ? diagnosisCode : $"{diagnosisCode}^{description}^I9";
            var dg1 = $"DG1|{setId}|I9|{diagnosis}";
            _segments.Add(dg1);
            return this;
        }

        public HL7MessageBuilder WithIN1(int setId, string planId, string companyId, string companyName)
        {
            var in1 = $"IN1|{setId}|{planId}|{companyId}|{companyName}";
            _segments.Add(in1);
            return this;
        }

        public HL7MessageBuilder WithSegment(string segmentData)
        {
            _segments.Add(segmentData);
            return this;
        }

        public Message Build()
        {
            var messageString = string.Join("\r", _segments);
            var message = new Message(messageString);
            message.ParseMessage();
            return message;
        }

        public string BuildString()
        {
            return string.Join("\r", _segments);
        }
    }
}