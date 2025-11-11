using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;
using System.IO;

/// <summary>
///  SceneLoaderWindow
/// 프로젝트 내 모든 씬을 검색해 목록으로 표시하고,
/// 클릭 한 번으로 바로 열거나(Load), 재생(Play)할 수 있게 해주는 Unity Editor 툴입니다.
///
/// 주요 기능:
/// - 모든 씬 자동 탐색 (AssetDatabase.FindAssets)
/// - 검색 필터로 씬 이름 빠른 검색
/// - 버튼으로 즉시 열기(Open) 또는 실행(Play)
/// </summary>
public class SceneLoaderWindow : EditorWindow
{
    // ScrollView 위치 저장용
    private Vector2 scrollPos;

    // 검색창 입력 문자열
    private string searchFilter = "";

    // 씬 파일의 전체 경로 (ex: Assets/Scenes/Main.unity)
    private string[] scenePaths;

    // 씬 이름 배열 (ex: Main)
    private string[] sceneNames;

    /// <summary>
    /// Unity 상단 메뉴에 "Tools/Scene Loader" 추가
    /// 클릭 시 창이 열림
    /// </summary>
    [MenuItem("Tools/Scene Loader")]
    public static void ShowWindow()
    {
        GetWindow<SceneLoaderWindow>("Scene Loader");
    }

    /// <summary>
    /// 에디터 창이 활성화될 때 호출됨
    /// 씬 리스트를 초기화
    /// </summary>
    private void OnEnable()
    {
        RefreshSceneList();
    }

    /// <summary>
    /// 프로젝트 내 모든 씬(.unity) 파일을 찾아 목록 갱신
    /// </summary>
    private void RefreshSceneList()
    {
        // t:Scene → 타입이 Scene인 Asset만 검색
        scenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath) // GUID → 실제 경로 변환
            .ToArray();

        // 경로에서 파일 이름(확장자 제외)만 추출
        sceneNames = scenePaths
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .ToArray();
    }

    /// <summary>
    /// 에디터 GUI 렌더링
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.Space();

        //검색창 영역
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));

        // 사용자가 입력한 검색어를 실시간 반영
        string newFilter = EditorGUILayout.TextField(searchFilter);
        if (newFilter != searchFilter)
        {
            searchFilter = newFilter;
        }

        // 새로고침 버튼
        if (GUILayout.Button("⟳", GUILayout.Width(30)))
        {
            RefreshSceneList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 스크롤 가능한 씬 리스트 영역
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // 씬 목록 반복 출력
        for (int i = 0; i < scenePaths.Length; i++)
        {
            // 검색 필터 적용 (대소문자 무시)
            if (!string.IsNullOrEmpty(searchFilter) &&
                !sceneNames[i].ToLower().Contains(searchFilter.ToLower()))
                continue;

            EditorGUILayout.BeginHorizontal();

            // 씬 이름 표시
            EditorGUILayout.LabelField(sceneNames[i]);

            // Open 버튼: 씬 열기
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                // 현재 씬이 수정된 경우, 저장 여부 묻기
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // 선택된 씬 열기
                    EditorSceneManager.OpenScene(scenePaths[i]);
                }
            }

            // Play 버튼: 씬 열고 바로 재생
            if (GUILayout.Button("Play", GUILayout.Width(60)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePaths[i]);
                    EditorApplication.isPlaying = true; // 재생 모드 시작
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
