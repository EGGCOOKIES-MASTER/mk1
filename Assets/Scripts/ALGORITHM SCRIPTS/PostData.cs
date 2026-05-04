using System;

/*
 PostData (간단한 버전)

 - 역할: 그리드에 표시할 게시물의 최소한의 정보만 담음
 - 필드: id, title, contentUrl (이미지 경로)
 - 확장: 나중에 신호(signals) 또는 캠페인 필드 추가 가능
 */

[Serializable]
public class PostData {
    // 필수 필드
    public string id;
    public string title;
    public string contentUrl; // 이미지/비디오 URL 또는 Resources 경로

    // 디버깅용 ToString
    public override string ToString() {
        return $"Post[{id}] title={title}";
    }
}

