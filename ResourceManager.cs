using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;        //여기서부터 추가하자
using Newtonsoft.Json;
using System.Text;

public class ResourceManager : MonoBehaviour
{
    //같은 타입 데이터는 같은 파일로 Append 하여 이어 쓴다
    //단, 인덱스(ID) 값이 같은 경우, 해당 인덱스의 내용을 수정한다
    //Json .Net 뭐시기 쓴다는 가정
    //CreateFile 메쏘오드를 호출해서 저장함
    //반드시반드시 저장할 객체에는 int형 필드인 ID가 있어야 함
    private static ResourceManager instance = null;
    public static ResourceManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        SetPath();
    }

    private static string path = string.Empty;
    private static void SetPath()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                path = string.Format("{0}/{1}", Application.dataPath, "Data");         //데이터 패스 정리본은 컨플에 ^^7
                Directory.CreateDirectory(path);
                break;
            case RuntimePlatform.WindowsPlayer:
                path = string.Format("{0}/{1}", Application.persistentDataPath, "Data");
                Directory.CreateDirectory(path);
                break;
        }
    }

    public static T ParseData<T>(string strData)
    {
        return JsonConvert.DeserializeObject<T>(strData);
    }

    public static string ToData<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    private static int GetLineByID(int id, string _path)
    {
        int line_toEdit = 1;

        if (!File.Exists(_path))
        {
            File.Create(_path);
            return line_toEdit;
        }

        using (StreamReader reader = new StreamReader(_path, Encoding.Default))
        {
            while (true)
            {
                if (reader.EndOfStream)
                {
                    return line_toEdit;
                }

                bool isID = false;
                string[] buffers = reader.ReadLine().Split('\"', ':', ',');         //\" 이렇게 하면 큰따옴표를 문자로 만들 수 있음
                foreach (var buffer in buffers)                                     //Json 텍스트는 이런 형태 {"type":value,~~~}
                {
                    if (isID)
                    {
                        if (buffer == string.Empty)
                        {
                            //버퍼가 텅 비었으면 다음 버퍼로 넘어감
                        }
                        else
                        {
                            if (int.Parse(buffer) == id)
                                return line_toEdit;

                            isID = false;
                        }
                    }
                    if (buffer == "ID")
                    {
                        isID = true;
                    }
                }

                line_toEdit++;
            }
        }
    }

    private static string fileNameExtension = "txt";
    public static void CreateFile<T>(T obj, int id, string fileName)
    {
        string _path = string.Format("{0}/{1}.{2}", path, fileName, fileNameExtension);

        try
        {
            int line_toEdit = GetLineByID(id, _path);       //수정할 줄을 찾음
            string line_src = ToData<T>(obj);
            string[] lines_dest = File.ReadAllLines(_path);

            using (StreamWriter writer = new StreamWriter(_path))
            {
                for (int i = 1; i <= lines_dest.Length + 1; ++i)        //인덱스가 1부터 시작함
                {
                    if (i == line_toEdit)                   //해당 줄일 경우, 수정할 문자열을 덮어씌움
                        writer.WriteLine(line_src);
                    else if (i == lines_dest.Length + 1)
                        break;
                    else
                        writer.WriteLine(lines_dest[i - 1]);    //해당 줄이 아닐 경우, 기존 문자열을 그대로 씀
                }
            }
        }
        catch (IOException e)
        {
            Debug.Log(e);
            return;
        }
    }
}
