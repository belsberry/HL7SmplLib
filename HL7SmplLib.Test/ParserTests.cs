using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HL7SmplLib;

namespace HL7SmplLib.Test
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void HL7Message_Constructor_Parsed_And_Not_Corrupted()
        {
            //arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string hl7Output = "";


            //act
            HL7Message messageObject = new HL7Message(hl7Input);//Constructor parses the HL7 message
            hl7Output = messageObject.Text; //.Text pieces everything back together.
            hl7Input = hl7Input.Trim(); //It is expected that trailing segment separators will be trimmed.

            //assert
            Assert.AreEqual(hl7Input, hl7Output);

        }

        [TestMethod]
        public void HL7Message_LoadHL7_Method_Parsed_And_Not_Corrupted()
        {
            //arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string hl7Output = "";
            HL7Message messageObject = new HL7Message();


            //act
            messageObject.LoadHL7(hl7Input);
            hl7Output = messageObject.Text; //.Text pieces everything back together.
            hl7Input = hl7Input.Trim(); //It is expected that trailing segment separators will be trimmed.

            //assert
            Assert.AreEqual(hl7Input, hl7Output);

        }

        [TestMethod]
        public void HL7Message_Indexer_Gets_First_Segment_And_Returns_Empty_If_Not_Found()
        {
            //arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string pidSegment = "PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|";
            string nteSegment = "NTE|1||Comment No. 1 attached to OBR";
            string nte2Segment = "NTE|1||Comment No. 1 attached to OBX";

            string pidOutput = "";
            string nteOutput = "";
            string gt1Output = "";


            //act
            HL7Message message = new HL7Message(hl7Input);

            pidOutput = message["PID"].Text;
            gt1Output = message["GT1"].Text;
            nteOutput = message["NTE"].Text;

            //assert
            Assert.AreEqual(pidSegment, pidOutput);
            Assert.IsTrue(String.IsNullOrEmpty(gt1Output));
            Assert.AreEqual(nteSegment, nteOutput);
            Assert.AreNotEqual(nte2Segment, nteOutput);
        }

        [TestMethod]
        public void HL7Message_Indexers_Return_Correct_Data()
        {
            //arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string msh_6_expected = "Client Facility";
            string msh_6_actual = "";

            string pid_5_2_expected = "Chris";
            string pid_5_2_actual = "";

            string obx_2_1_1_expected = "ValueType";
            string obx_2_1_1_actual = "";

            string obx_2_5_1_expected = "";//should be empty
            string obx_2_5_1_actual = "";

            string nte_3_1_1_expected = "Comment No. 1 attached to OBR";
            string nte_3_1_1_actual = "";

            //act
            HL7Message message = new HL7Message(hl7Input);
            msh_6_actual = message["MSH"][6].Text;
            pid_5_2_actual = message["PID"][5][2].Text;
            obx_2_1_1_actual = message["OBX"][2][1][1].Text;
            obx_2_5_1_actual = message["OBX"][2][5][1].Text;
            nte_3_1_1_actual = message["NTE"][3][1][1].Text;


            //assert
            Assert.AreEqual(msh_6_expected, msh_6_actual, "MSH is wrong");
            Assert.AreEqual(pid_5_2_expected, pid_5_2_actual, "PID is wrong");
            Assert.AreEqual(obx_2_1_1_expected, obx_2_1_1_actual, "OBX Full node is wrong");
            Assert.AreEqual(obx_2_5_1_expected, obx_2_5_1_actual, "OBX Empty node is wrong");
            Assert.AreEqual(nte_3_1_1_expected, nte_3_1_1_actual, "NTE is wrong");
        }

        [TestMethod]
        public void Can_Tell_If_HL7()
        {
            //arrange
            string input1 = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|";
            string input2 = "<xml></xml>";
            string input3 = "here is a bunch of text";
            string input4 = @"SSH|^~\&|eTX HEMI|Sending Facility|Client Application";

            //assert
            Assert.IsTrue(HL7Message.IsHL7(input1));
            Assert.IsFalse(HL7Message.IsHL7(input2));
            Assert.IsFalse(HL7Message.IsHL7(input3));
            Assert.IsFalse(HL7Message.IsHL7(input4));
        }

        [TestMethod]
        public void Indexers_Do_Not_Cause_Exception_And_Return_Empty_When_Not_Found()
        {
            //arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string nte16 = "";
            string nte3dot16 = "";
            string nte3dot1dot16 = "";


            //act
            HL7Message message = new HL7Message(hl7Input);
            nte16 = message["NTE"][16].Text;
            nte3dot16 = message["NTE"][3][16].Text;
            nte3dot1dot16 = message["NTE"][3][1][16].Text;

            //Assert
            Assert.IsTrue(String.IsNullOrEmpty(nte16));
            Assert.IsTrue(String.IsNullOrEmpty(nte3dot16));
            Assert.IsTrue(String.IsNullOrEmpty(nte3dot1dot16));
        }

        [TestMethod]
        public void Segment_Is_Empty_Reports_Correctly()
        {
            //arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
BLA|||||
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            HL7Segment NTE = null;
            HL7Segment GT1 = null;
            HL7Segment BLA = null;

            //act
            HL7Message message = new HL7Message(hl7Input);
            NTE = message["NTE"];
            GT1 = message["GT1"];
            BLA = message["BLA"];

            //Assert
            Assert.IsTrue(GT1.IsEmpty);
            Assert.IsFalse(NTE.IsEmpty);
            Assert.IsFalse(BLA.IsEmpty);
        }

        [TestMethod]
        public void HL7Field_IsEmpty_Reports_Correctly()
        {
            //Arrange
            string field1 = "";
            string field2 = "^^^~^^&&^";
            string field3 = "^^this is not empty^^^^";

            HL7Field f1 = new HL7Field(field1, new HL7CharSet());
            HL7Field f2 = new HL7Field(field2, new HL7CharSet());
            HL7Field f3 = new HL7Field(field3, new HL7CharSet());

            //Assert
            Assert.IsTrue(f1.IsEmpty);
            Assert.IsTrue(f2.IsEmpty);
            Assert.IsFalse(f3.IsEmpty);


        }

        [TestMethod]
        public void HL7Segment_Can_Parse_Independently_Via_Constructor()
        {
            //Arrange
            string pidSegment = "PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|";
            
            //Act
            HL7Segment pid = new HL7Segment(pidSegment);

            //Assert
            Assert.AreEqual("1", pid[3].Text);
            Assert.AreEqual("Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2", pid[5].Text);
            Assert.AreEqual("Chris", pid[5][2].Text);
            Assert.IsTrue(String.IsNullOrEmpty(pid[5][6].Text));
            Assert.AreEqual(pidSegment, pid.Text);

        }

        [TestMethod]
        public void HL7Segment_Can_Parse_With_Different_Chars()
        {
            //Arrange
            string pidSegment = "PID!!!1!!Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2!!!M!!!724 Hickory St^NULL^Springfield^OH^55555^US!!!!!!!!111-00-1234!";

            //Act
            HL7Segment pid = new HL7Segment(pidSegment, new HL7CharSet('!', '^', '&', '~'));

            //Assert
            Assert.AreEqual("1", pid[3].Text);
            Assert.AreEqual("Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2", pid[5].Text);
            Assert.AreEqual("Chris", pid[5][2].Text);
            Assert.IsTrue(String.IsNullOrEmpty(pid[5][6].Text));
            Assert.AreEqual(pidSegment, pid.Text);

        }

        [TestMethod]
        public void HL7Field_Can_Parse_Repeats()
        {
            //Arrange
            string field = "Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2";

            //Act
            HL7Field objField = new HL7Field(field, new HL7CharSet());

            //Assert
            Assert.AreEqual("Bacon", objField[1].Text);
            Assert.AreEqual("P", objField[3].Text);
            Assert.AreEqual("", objField[6].Text);
            
        }

        [TestMethod]
        public void HL7Field_Can_Return_Repeat_At_Index()
        {
            //Arrange
            string field = "Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2";
            
            //Act
            HL7Field objField = new HL7Field(field, new HL7CharSet());

            //Assert
            Assert.AreEqual(3, objField.Repeats.Length);
            
            HL7Field repeat = objField.GetRepeatNumber(1);
            

            Assert.AreEqual("Bacon1", repeat[1].Text);
            Assert.AreEqual("Chris1", repeat[2].Text);
            Assert.AreEqual("P1", repeat[3].Text);
            


        }

        [TestMethod]
        public void HL7Field_Can_Report_Repeating()
        {
            //Arrange
            string field = "Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2";

            //Act
            HL7Field objField = new HL7Field(field, new HL7CharSet());

            //Assert
            Assert.IsTrue(objField.IsRepeating);
            HL7Field r1 = objField.GetRepeatNumber(0);
            Assert.IsFalse(r1.IsRepeating);
        }

        [TestMethod]
        public void HL7Field_Can_Return_Repeating_Fields_If_Not_Repeating()
        {
            //Arrange
            string field = "Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2";

            //Act
            HL7Field objField = new HL7Field(field, new HL7CharSet());

            //Assert
            HL7Field[] repeats = objField.Repeats;
            Assert.AreEqual(3, repeats.Length);

            HL7Field[] repeatSub = repeats[2].Repeats;
            Assert.AreEqual(1, repeatSub.Length);
            HL7Field sub = repeatSub[0];
            Assert.AreEqual("Bacon2", sub[1].Text);
            Assert.AreEqual("Chris2", sub[2].Text);
            Assert.AreEqual("P2", sub[3].Text);
            
        }

        [TestMethod]
        public void Can_Remove_Segment_By_Assigning_Empty()
        {
            //Arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            string expectedOutput = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string expectedOutput2 = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10";

            string expectedOutput3 = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10";

            HL7Message message = new HL7Message(hl7Input);

            //Act
            message["PV1"].Text = "";
            

            //Assert
            Assert.IsTrue(String.IsNullOrEmpty(message["PV1"].Text));
            Assert.IsTrue(message["PV1"].IsEmpty);

            string txt = message.Text;
            Assert.AreEqual(expectedOutput, txt);

            //Act again
            HL7Segment[] nteSegs = message.GetSegmentsByType("NTE");
            foreach (HL7Segment seg in nteSegs)
            {
                seg.Text = "";
            }

            //Act again
            foreach (HL7Segment seg in nteSegs)
            {
                Assert.IsTrue(seg.IsEmpty);
            }

            //Assert
            Assert.AreEqual(expectedOutput2, message.Text);

            //Act again
            message = new HL7Message(hl7Input);
            nteSegs = message.AllSegmentsWhere(s => s.Type == "NTE");

            foreach (HL7Segment seg in nteSegs)
            {
                seg.Text = "";
            }

            //Assert
            Assert.AreEqual(expectedOutput3, message.Text);

            
        }


    }
}

