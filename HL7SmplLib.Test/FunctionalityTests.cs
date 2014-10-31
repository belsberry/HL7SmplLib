using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HL7SmplLib.Test
{
    [TestClass]
    public class FunctionalityTests
    {
        [TestMethod]
        public void HL7Field_Can_Return_First_Field_Based_On_Func()
        {
            //Arrange
            string field = "Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2";

            //Act
            HL7Field objField = new HL7Field(field, new HL7CharSet());//Parse the field

            HL7Field whereP1 = objField.RepeatWhere(f => f[3].Text == "P1");
            HL7Field whereP5 = objField.RepeatWhere(f => f[3].Text == "P5");


            //Assert

            Assert.IsFalse(whereP1.IsEmpty);
            Assert.AreEqual("Chris1", whereP1[2].Text);
            Assert.IsTrue(whereP5.IsEmpty);
            Assert.AreEqual("", whereP5[2].Text);


        }

        [TestMethod]
        public void HL7Field_Can_Return_AllRepeatsWhere()
        {
            //Arrange
            string field = "Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2";

            //Act
            HL7Field objField = new HL7Field(field, new HL7CharSet());

            HL7Field[] whereP1P2 = objField.AllRepeatsWhere(f => f[3].Text == "P1" || f[3].Text == "P2");
            HL7Field[] whereP5 = objField.AllRepeatsWhere(f => f[3].Text == "P5");
            HL7Field[] whereNotP1 = objField.AllRepeatsWhere(f => f[3].Text != "P1");
            
            //Assert

            //Assert.IsFalse(whereP1P2.IsEmpty);
            Assert.AreEqual(2, whereP1P2.Length);
            Assert.AreEqual("Chris1", whereP1P2[0][2].Text);
            Assert.AreEqual("Chris2", whereP1P2[1][2].Text);
            Assert.AreEqual(0, whereP5.Length);
            Assert.AreEqual(2, whereNotP1.Length);
            //Assert.AreEqual("", whereP5[].Text);

            //Act Again
            foreach (HL7Field whereField in whereP1P2)
            {
                whereField[1].Text = "Changed";
            }

            string afterChange = objField.Text;
            string expected = "Bacon^Chris^P~Changed^Chris1^P1~Changed^Chris2^P2";

            //Assert Again
            Assert.AreEqual(expected, afterChange);
        }

        [TestMethod]
        public void Can_Get_Segment_With_Func_Predicate()
        {
            //Arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            HL7Message msg = new HL7Message(hl7Input);

            //Act
            HL7Segment nteSeg = msg.FirstSegmentWhere(s => s.Type == "NTE" && s[1].Text == "2");
            HL7Segment obxSeg = msg.FirstSegmentWhere(s => s.Type == "OBX" && s[2].Text == "ValueToCheck");

            //Assert
            Assert.AreEqual("Comment No. 2 attached to OBX", nteSeg[3].Text);
            Assert.AreEqual("Here it is", obxSeg[4].Text);

        }

        [TestMethod]
        public void Can_Get_All_Segments_With_Func_Predicate()
        {
            //Arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            HL7Message msg = new HL7Message(hl7Input);

            //Act
            HL7Segment[] nteSegs = msg.AllSegmentsWhere(s => s.Type == "NTE" && s[3].Text == "Comment No. 1 attached to OBX");
            
            //Assert
            Assert.AreEqual(2, nteSegs.Length);
            Assert.AreEqual("NTE|1||Comment No. 1 attached to OBX", nteSegs[0].Text);

        }

        [TestMethod]
        public void Can_Insert_Segment_At_Index()
        {
            //Arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string expectedOutput = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
RTE|1|Blah
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            

            HL7Message msg = new HL7Message(hl7Input);
            HL7Segment seg = new HL7Segment("RTE|1|Blah", msg.CharSet);
            HL7Message msg2 = new HL7Message(hl7Input);
            

            //Act
            msg.InsertSegment(seg, 5);
            msg2.InsertSegment("RTE|1|Blah", 5);
            

            //Assert
            Assert.AreEqual(expectedOutput, msg.Text);
            Assert.AreEqual(expectedOutput, msg2.Text);
            
        }

        [TestMethod]
        public void Can_Insert_After_Segment()
        {

            //Arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            string expectedOutput1 = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
RTE|1|New Segment
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            string expectedOutput2 = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
RTE|1|New Segment
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            string expectedOutput3 = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
RTE|1|New Segment
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";

            HL7Message msg1 = new HL7Message(hl7Input);
            HL7Segment seg1 = new HL7Segment("RTE|1|New Segment", msg1.CharSet);
            HL7Message msg2 = new HL7Message(hl7Input);
            HL7Segment seg2 = new HL7Segment("RTE|1|New Segment", msg2.CharSet);
            HL7Message msg12 = new HL7Message(hl7Input);
            HL7Message msg22 = new HL7Message(hl7Input);
            HL7Message msg3 = new HL7Message(hl7Input);
            
            //Act
            msg1.InsertSegment(seg1, "NTE");
            msg2.InsertSegment(seg2, "NTE", 4);
            msg12.InsertSegment("RTE|1|New Segment", "NTE");
            msg22.InsertSegment("RTE|1|New Segment", "NTE", 4);
            msg3.InsertSegment(seg1, "OBX", -1);

            //Assert
            Assert.AreEqual(msg1.Text, expectedOutput1);
            Assert.AreEqual(msg2.Text, expectedOutput2);
            Assert.AreEqual(msg12.Text, expectedOutput1);
            Assert.AreEqual(msg22.Text, expectedOutput2);
            Assert.AreEqual(msg3.Text, expectedOutput3);

        }

        [TestMethod]
        public void Can_Get_Correct_Indices()
        {
            //Arrange
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            HL7Message msg = new HL7Message(hl7Input);

            //Act
            int ndx1 = msg.GetIndexOfSegment("NTE");//Should be 4
            int ndx2 = msg.GetIndexOfSegment("OBR");//Should be 3
            int ndx3 = msg.GetIndexOfSegment("OBX", 2); //Should be 10
            int ndx4 = msg.GetLastIndexOfSegment("OBX");//Should be 10
            int ndx5 = msg.GetLastIndexOfSegment("NTE", -3); //Should be 9

            //Assert
            Assert.AreEqual(4, ndx1);
            Assert.AreEqual(3, ndx2);
            Assert.AreEqual(10, ndx3);
            Assert.AreEqual(10, ndx4);
            Assert.AreEqual(9, ndx5);


        }

        [TestMethod]
        public void Can_Get_HL7Objects_By_Location()
        {
            string hl7Input = @"MSH|^~\&|eTX HEMI|Sending Facility|Client Application|Client Facility|2010-09-12T14:17:16.190||ORU^R01|2010-09-12T14:17:16.190|P|2.3|
PID|||1||Bacon^Chris^P&Bacon2~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|
PV1||defaultPatientClass|||||123_PhysicianID^Quack^Hiram||||||||||446_ClientID||1|||||||||||||||||||||||||2009-01-08T00:00:00.000|
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueType|ObservationIdentifier|ObservationSubID|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX
OBR|1|12345.pdf|FillersOrder|UniversalServiceID|Priority|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|2010-09-12T14:17:16.190|CollectionVolume_Quantity|CollectorIdentifier_IDNumber
NTE|1||Comment No. 1 attached to OBR
OBX|1|ValueToCheck|ObservationIdentifier|Here it is|ObservationValue|Units_Identifier_Group|ReferenceRange|AbnormalFlag|Probability|OBX10
NTE|1||Comment No. 1 attached to OBX
NTE|2||Comment No. 2 attached to OBX";
            HL7Message msg = new HL7Message(hl7Input);
            HL7Segment seg1 = msg.GetSegmentByLocation("PID");
            HL7Segment seg2 = msg.GetSegmentByLocation("PID.1.2.2.4.5.2.1.3");//test crazy things.
            HL7Field field1 = msg.GetFieldByLocation("PID");//should be the first field of the segment
            HL7Field field2 = msg.GetFieldByLocation("PID.3");
            HL7Field field3 = msg.GetFieldByLocation("PID.3.1.2.23.4.5");//test crazy input
            HL7Component comp1 = msg.GetComponentByLocation("PID.5");//should return the first comp of PID.5
            HL7Component comp2 = msg.GetComponentByLocation("PID.5.3");
            HL7Component comp3 = msg.GetComponentByLocation("PID.5.3.2.3.4.5.6.7.");//Crazy
            HL7Subcomponent sub1 = msg.GetSubcomponentByLocation("PID.5");//should get first subcomponent of first comp of PID.5
            HL7Subcomponent sub2 = msg.GetSubcomponentByLocation("PID.5.3.2");
            HL7Subcomponent sub3 = msg.GetSubcomponentByLocation("PID.5.3.2.1.2.3.4.5.6.7");

            //Assert
            Assert.AreEqual("PID|||1||Bacon^Chris^P&Bacon2~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|", seg1.Text);
            Assert.AreEqual("PID|||1||Bacon^Chris^P&Bacon2~Bacon1^Chris1^P1~Bacon2^Chris2^P2|||M|||724 Hickory St^NULL^Springfield^OH^55555^US||||||||111-00-1234|", seg2.Text);
            Assert.AreEqual("", field1.Text);
            Assert.AreEqual("1", field2.Text);
            Assert.AreEqual("1", field3.Text);
            Assert.AreEqual("Bacon", comp1.Text);
            Assert.AreEqual("P&Bacon2", comp2.Text);
            Assert.AreEqual("P&Bacon2", comp2.Text);
            Assert.AreEqual("Bacon", sub1.Text);
            Assert.AreEqual("Bacon2", sub2.Text);
            Assert.AreEqual("Bacon2", sub3.Text);

        }

    }
}
