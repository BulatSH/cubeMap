using UnityEngine;
using System.Collections;
using System.IO;

public class CubemapProbe : MonoBehaviour {

	int resolution = 512;
	
	#region Cubemap functions

	private Texture2D CaptureScreen(int destX, int destY) {
		Texture2D result;
		Rect captureZone = new Rect( 0f, 0f, Screen.width, Screen.height );
		//result = new Texture2D( Mathf.RoundToInt(captureZone.width), Mathf.RoundToInt(captureZone.height), TextureFormat.RGB24, false);
		result = new Texture2D( Mathf.RoundToInt( captureZone.width ) + destX,
		                       Mathf.RoundToInt( captureZone.height ) + destY,
		                       TextureFormat.RGB24, false);
		result.ReadPixels(captureZone, 0, 0, false);
		result.Apply();
		return result;
	}
	private void SaveTextureToFile(Texture2D texture, string fileName) {
		byte[] bytes = texture.EncodeToPNG();
		FileStream file = File.Open(Application.dataPath + "/" + fileName,FileMode.Create);
		BinaryWriter binary = new BinaryWriter(file);
		binary.Write(bytes);
		file.Close();
	}
	Texture2D Resize(Texture2D sourceTex, int Width, int Height) {
		Texture2D destTex = new Texture2D(Width, Height, sourceTex.format, true);
		Color[] destPix = new Color[Width * Height];
		int y = 0;
		while (y < Height) {
			int x = 0;
			while (x < Width) {
				float xFrac = x * 1.0F / (Width );
				float yFrac = y * 1.0F / (Height);
				destPix[y * Width + x] = sourceTex.GetPixelBilinear(xFrac, yFrac);
				x++;
			}
			y++;
		}
		destTex.SetPixels(destPix);
		destTex.Apply();
		return destTex;
	}
	Texture2D Flip(Texture2D sourceTex) {
		Texture2D Output = new Texture2D(sourceTex.width, sourceTex.height, sourceTex.format, true);
		for (int y = 0; y < sourceTex.height; y++)
		{
			for (int x = 0; x < sourceTex.width; x++)
			{
				Color pix = sourceTex.GetPixel(sourceTex.width + x, (sourceTex.height-1) - y);
				Output.SetPixel(x, y, pix);
			}
		}
		return Output;
	}
	#endregion
	void Start () {
		StartCoroutine(CreateCubeMap());


	}
	public Cubemap cm;
	IEnumerator CreateCubeMap()
	{	cm = new Cubemap(resolution, TextureFormat.RGB24, true);
		//cm = (Cubemap)AssetBundle.FindObjectOfType("firtCubeMap");
		if(GetComponent<Renderer>()) {
			GetComponent<Renderer>().enabled = false;
		}
		Quaternion[] rotations = { Quaternion.Euler(-90,0,180), Quaternion.Euler(0,90,180), Quaternion.Euler(0,0,180), Quaternion.Euler(90,0,180), Quaternion.Euler(0,-90,180), Quaternion.Euler(0,180,180)};
		CubemapFace[] faces = { CubemapFace.PositiveY, CubemapFace.PositiveX, CubemapFace.PositiveZ, CubemapFace.NegativeY, CubemapFace.NegativeX, CubemapFace.NegativeZ };

		Texture2D face = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
		
		face.wrapMode = TextureWrapMode.Clamp;
		
		GameObject go = new GameObject("CubemapCamera", typeof(Camera));
		
		go.transform.position = transform.position;
		
		go.transform.rotation = Quaternion.identity;
		
		go.GetComponent<Camera>().fieldOfView = 90;
		
		go.GetComponent<Camera>().depth = float.MaxValue;
		
		for(int i = 0; i < 6; i++) {
			go.transform.rotation = rotations[i];
			yield return new WaitForEndOfFrame();
			face = CaptureScreen(0, 0);
			face = Resize(face, resolution, resolution);
			face = Flip(face);
			Color[] faceColours = face.GetPixels();
			cm.SetPixels(faceColours, faces[i], 0);
			SaveTextureToFile(face, faces[i].ToString() + ".png");

		}


		
		
		cm.Apply();
		Texture2D texture = GetImage (cm, faces);
		SaveTextureToFile (texture, "texture.png");
		if(GetComponent<Renderer>().material.HasProperty("Main Camera")) {
			GetComponent<Renderer>().material.SetTexture("Main Camera", cm);
		}
		
		DestroyImmediate(face);
		DestroyImmediate(go);
		
		if(GetComponent<Renderer>()) {
			GetComponent<Renderer>().enabled = true;
		}
	} 

	//Cubemap cubemap;

	Texture2D GetImage(Cubemap cube, CubemapFace[] faces) {

		Texture2D texture = new Texture2D (512, 512);
		for (int i = 0; i < faces.Length; i++) {
			CubemapFace face = faces[i];
			Color[] faceColors = cube.GetPixels(face, 0);
			texture.SetPixels(faceColors);
		}
		texture.Apply ();
		return texture;
	}

}