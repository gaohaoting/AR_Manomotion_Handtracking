using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public enum BezierPackageAction {
    kPackageActionEndSpline = 0,
    kPackageActionCreateContinuingSpline = 1,
	kPackageActionSplitSpline = 2,
	kPackageActionRemovePointFromSpline = 3,
	kPackageActionAddPoint = 4,
	kPackageActionSetLastPoint = 5,
	kPackageActionUndo = 6,
	kPackageActionUndoAll = 7,
	kPackageActionRedo = 8,
    kPackageCameraPosition = 9,
    kPackageLeftControllerPosition = 10,
    kPackageRightControllerPosition = 11,
    kPackageBrushUpdate = 12,
	kPackageTimeScaleFactor = 13
}

[Serializable]
public class BezierPackage : IComparable {
	public const uint PackageByteSize = 68;

    // message meta
    public BezierPackageAction action; // 4

    // spline data
    public Int32 splineId = -1; // 4
	public Int32 brushIndex = -1; // 4
	public Color32 color = new Color32(0,0,0,0); // 4

	// point data
	public Vector3 pointPosition; // 4*3
	public Quaternion pointInitialRotation; // 4*4
	public Vector3 pointVelocity; // 4*3
	public Single timeAbsolute; // 4
	public Single pointWidthPostClampScaleFactor; // 4
	public Single pointCustomParameter; // 4




	// CUSTOM SERIALIZATION
	private static void SerializeColor32(BinaryWriter writer, Color32 c) {
		writer.Write ((Byte)c.r);
		writer.Write ((Byte)c.g);
		writer.Write ((Byte)c.b);
		writer.Write ((Byte)c.a);
	}
	private static Color32 DeserializeColor32(BinaryReader reader) {
		return new Color32 (reader.ReadByte (), reader.ReadByte (), reader.ReadByte (), reader.ReadByte());
	}
	private static void SerializeVector3(BinaryWriter writer, Vector3 v) {
		writer.Write((Single)v.x);
		writer.Write((Single)v.y);
		writer.Write((Single)v.z);
	}
	private static Vector3 DeserializeVector3(BinaryReader reader) {
		return new Vector3 (reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle ());
	}
	private static void SerializeQuaternion(BinaryWriter writer, Quaternion q) {
		writer.Write((Single)q.x);
		writer.Write((Single)q.y);
		writer.Write((Single)q.z);
		writer.Write((Single)q.w);
	}
	private static Quaternion DeserializeQuaternion(BinaryReader reader) {
		return new Quaternion (reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle ());
	}

	public byte[] Serialize() {
		using (MemoryStream m = new MemoryStream()) {
			using (BinaryWriter writer = new BinaryWriter(m))
            {
                // meta
				writer.Write((Int32)action);

                // spline
				writer.Write ((Int32)splineId);
				writer.Write ((Int32)brushIndex);
				SerializeColor32 (writer, color);

				// point
				SerializeVector3(writer, pointPosition);
				SerializeQuaternion (writer, pointInitialRotation);
				SerializeVector3 (writer, pointVelocity);
				writer.Write ((Single)timeAbsolute);
				writer.Write ((Single)pointWidthPostClampScaleFactor);
				writer.Write ((Single)pointCustomParameter);
			}
			return m.ToArray();
		}
	}

	public static BezierPackage Desserialize(byte[] data, int offset) {
		BezierPackage result = new BezierPackage();
		using (MemoryStream m = new MemoryStream(data)) {
			using (BinaryReader reader = new BinaryReader(m))
            {
                // meta
                result.action = (BezierPackageAction)reader.ReadInt32();

                // spline
                reader.ReadBytes(offset);
				result.splineId = reader.ReadInt32();
				result.brushIndex = reader.ReadInt32 ();
				result.color = DeserializeColor32 (reader);

				// point
				result.pointPosition = DeserializeVector3(reader);
				result.pointInitialRotation = DeserializeQuaternion(reader);
				result.pointVelocity = DeserializeVector3(reader);
				result.timeAbsolute = reader.ReadSingle ();
				result.pointWidthPostClampScaleFactor = reader.ReadSingle ();
				result.pointCustomParameter = reader.ReadSingle ();
			}
		}
		return result;
	}


    // SOR BY TIME
    public int CompareTo(object other)
    {
        if (other == null)
            return 1;

		BezierPackage otherPackage = other as BezierPackage;
        if (otherPackage == null)
        {
            throw new ArgumentException("Could not compare package times.");
        }

        return timeAbsolute.CompareTo(otherPackage.timeAbsolute);
    }
}

