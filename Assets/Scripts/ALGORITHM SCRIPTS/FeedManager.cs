using System;
using System.Collections.Generic;
using UnityEngine;

/*
 FeedManager (간단한 버전)

 - 역할: 샘플 포스트를 생성하고 순서대로 페이징 반환
 - 간단함: 스코어링 없음, 순서대로 제공
 */
public class FeedManager : MonoBehaviour {
    public static FeedManager Instance { get; private set; }

    [Header("Prototype data")]
    [Tooltip("로컬에서 생성할 샘플 포스트 개수(프로토타입용)")]
    public int samplePostCount = 30;

    // 내부 저장소: 실제 환경에서는 서버에서 받아오거나 로컬 DB를 사용
    private List<PostData> allPosts = new List<PostData>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        // 샘플 데이터 생성(프로토타입)
        GenerateSamplePosts(samplePostCount);
    }

    /// <summary>
    /// 간단한 샘플 포스트 생성
    /// </summary>
    public void GenerateSamplePosts(int count) {
        allPosts.Clear();
        for (int i = 0; i < count; i++) {
            var p = new PostData();
            p.id = "post_" + i.ToString("000");
            p.title = "샘플 포스트 " + (i + 1);
            p.contentUrl = string.Empty; // 프로토타입에서는 실제 URL 사용 안함
            allPosts.Add(p);
        }
    }

    /// <summary>
    /// 페이지 단위로 포스트 반환 (순서대로)
    /// pageIndex: 0부터 시작
    /// </summary>
    public List<PostData> GetNextPage(int pageSize, int pageIndex) {
        if (allPosts == null || allPosts.Count == 0) return new List<PostData>();

        // 단순 페이징: 순서대로 반환
        int skip = pageIndex * pageSize;
        var page = new List<PostData>();
        for (int i = skip; i < skip + pageSize && i < allPosts.Count; i++) {
            page.Add(allPosts[i]);
        }
        return page;
    }

    /// <summary>
    /// 첫 페이지(편의 메서드)
    /// </summary>
    public List<PostData> GetFirstPage(int pageSize) {
        return GetNextPage(pageSize, pageIndex: 0);
    }
}

