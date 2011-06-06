/* Simples XNA Content Pipeline Importer for BYU files
 * http://code.google.com/p/xna-byuimporter/
 * 
 * @author: Rômulo Penido | romulo DOT penido AT gmail DOT com
 * 
 * This program is free software. It comes without any warranty, to
 * the extent permitted by applicable law. You can redistribute it
 * and/or modify it under the terms of the Do What The Fuck You Want
 * To Public License, Version 2, as published by Sam Hocevar. See
 * http://sam.zoy.org/wtfpl/COPYING for more details. */



using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Windows.Forms;
using System.IO;

namespace Simples.Content.Pipeline.STLImporter
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// </summary>
    [ContentImporter(".g", DisplayName = "BYUImporter", DefaultProcessor = "ModelProcessor")]
    public class BYUImporter : ContentImporter<NodeContent>
    {

        private MeshBuilder meshBuilder;

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            System.Diagnostics.Debugger.Launch();
            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            
            NodeContent root = new NodeContent();            
            string[] header = sr.ReadLine().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            int partCount = int.Parse(header[0]);
            int vertCount = int.Parse(header[1]);
            int faceCount = int.Parse(header[2]);
            // int edgeCount = int.Parse(header[3]); // We dont need that

            sr.ReadLine(); // Skip second line
            
            if (partCount != 1)
                throw new InvalidDataException("Invalid BYU file");
            
            meshBuilder = MeshBuilder.StartMesh("");
            meshBuilder.SwapWindingOrder = true;

            int[] verticesIndex = new int[vertCount];
            int iVert = 0;
            float[] vertCoords = new float[3];

            int iCoord = 0;
            while (iVert < vertCount)
            {
                string[] vertRaw = sr.ReadLine().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                /*
                if (vertRaw.Length % 3 == 0)
                    throw new InvalidDataException("Invalid BYU file");
                 */ 
                foreach (string coordRaw in vertRaw)
                {
                    vertCoords[iCoord] = float.Parse(coordRaw);
                    iCoord++;
                    if (iCoord % 3 == 0)
                    {
                        Vector3 vertex = new Vector3(vertCoords[0], vertCoords[1], vertCoords[2]);
                        verticesIndex[iVert] = meshBuilder.CreatePosition(vertex);
                        iVert++;
                        iCoord = 0;
                    }
                }
            }

        
            for (int iFace = 0; iFace < faceCount; iFace++)
            {
                string[] faceRaw = sr.ReadLine().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                
                if (faceRaw.Length != 3)
                    throw new InvalidDataException("Invalid BYU file");

                int v1 = int.Parse(faceRaw[0])-1;
                int v2 = int.Parse(faceRaw[1])-1;
                int v3 = -int.Parse(faceRaw[2])-1;

                if (v3 < 0)
                    throw new InvalidDataException("Invalid BYU file");
                
                meshBuilder.AddTriangleVertex(verticesIndex[v1]);
                meshBuilder.AddTriangleVertex(verticesIndex[v2]);
                meshBuilder.AddTriangleVertex(verticesIndex[v3]);
            }
            
            MeshContent mesh = meshBuilder.FinishMesh();

            root.Children.Add(mesh);

           return root;
        }
    }
}
