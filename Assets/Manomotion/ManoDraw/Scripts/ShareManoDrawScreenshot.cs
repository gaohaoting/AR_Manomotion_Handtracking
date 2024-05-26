using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShareManoDrawScreenshot : MonoBehaviour
{
	[SerializeField]
	private Canvas iconManagerCanvas;

	public void OnClickShareButton()
    {
		StartCoroutine(TakeScreenshotAndShare());
		
	}

	public IEnumerator TakeScreenshotAndShare()
	{
		yield return null;

		iconManagerCanvas.enabled = false;

		yield return new WaitForEndOfFrame();

		Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		ss.Apply();

		string filePath = Path.Combine(Application.temporaryCachePath, "manoDrawScreenshot.png");
		File.WriteAllBytes(filePath, ss.EncodeToPNG());

		// To avoid memory leaks
		Destroy(ss);

		//new NativeShare().AddFile(filePath)
		//	.SetSubject("ManoDraw Screenshot").SetText("Look what I have drawn with ManoDraw! #manomotion #manodraw #handtracking")
		//	.SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
		//	.Share();

		iconManagerCanvas.enabled = true;
	}
}
