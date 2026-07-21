using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// ★ 회사 환경 교체 지점 — Pending Requests 화면의 서버 처리(수신/Create/강제
    /// 수신)와 발송 공장 코드 조회를 modernlab-api(REST)로 호출하는 단일 클라이언트.
    ///
    /// 홈 환경은 이 클래스가 http://localhost:8080/api/pending/* 로 요청하고, 서버는
    /// FAC_SEND_MAS / IF_REQ_MAS(Oracle)에 처리 시각(SYSDATE)과 함께 적재한다.
    /// 화면은 처리 성공 후 재조회로 그 값을 받아 보여준다(화면이 시각을 만들지 않음).
    ///
    /// 회사 적용 시 이 클래스의 각 메서드 본문만 회사 인터페이스(ITEM 생성 전문,
    /// 의뢰 처리 갱신, 수신 전문, 공장 코드 조회) 호출로 바꾸면 된다 — 폼/다이얼로그
    /// 코드는 그대로 둔다.
    /// </summary>
    internal static class PendingApiClient
    {
        // 홈 환경 API 주소/제한 시간 — 회사 적용 시 함께 제거한다.
        private const string apiBaseUrl = "http://localhost:8080";
        private const int apiTimeoutMs = 5000;

        /// <summary>서버 처리 응답 — 성공 여부와 실패 사유.</summary>
        internal sealed class ActionResult
        {
            /// <summary>처리 성공 여부.</summary>
            internal bool Success;

            /// <summary>실패 사유 (성공이면 빈 문자열) — 화면 표기용 영어 문구.</summary>
            internal string Message;
        }

        /// <summary>제한 시간을 적용한 WebClient (홈 환경 전용 헬퍼).</summary>
        private sealed class TimedWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                request.Timeout = apiTimeoutMs;
                return request;
            }
        }

        /// <summary>
        /// 발송 공장 코드 목록 — 매뉴얼 Receive 다이얼로그의 콤보 원천.
        /// ★ 회사 적용 시 공장 코드 마스터 조회로 교체한다.
        /// </summary>
        internal static string[] GetSendFacilities()
        {
            using (WebClient client = new TimedWebClient())
            {
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString(apiBaseUrl + "/api/pending/send-facilities");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Deserialize<string[]>(json) ?? new string[0];
            }
        }

        /// <summary>
        /// 수신 처리 — 도착 건의 RECV_YN/RECV_TM/상태를 채운다.
        /// ★ 회사 적용 시 ITEM 생성 인터페이스 호출로 바꾼다.
        /// </summary>
        internal static ActionResult Receive(string itemId)
        {
            return Post("/api/pending/receive", NewBody("itemId", itemId));
        }

        /// <summary>
        /// Create 처리 — 의뢰 인터페이스의 PROC_YN/PROC_TM을 채운다.
        /// ★ 회사 적용 시 의뢰 처리 인터페이스 호출로 바꾼다.
        /// </summary>
        internal static ActionResult Create(string itemId)
        {
            return Post("/api/pending/create", NewBody("itemId", itemId));
        }

        /// <summary>
        /// 강제 수신 단일 전문 — 조회에 안 나오는 아이템을 Item ID + 발송 공장
        /// 입력으로 수신 처리한다. 서버가 체크(중복 수신 거부)와 처리를 한 번에
        /// 수행하고 실패면 사유를 돌려준다.
        /// ★ 회사 적용 시 수신 전문 호출로 바꾼다.
        /// </summary>
        internal static ActionResult ManualReceive(string itemId, string sendFac)
        {
            Dictionary<string, string> body = new Dictionary<string, string>();
            body["itemId"] = itemId ?? string.Empty;
            body["sendFac"] = sendFac ?? string.Empty;
            return Post("/api/pending/manual-receive", body);
        }

        // 인자 하나짜리 요청 본문 구성 헬퍼.
        private static Dictionary<string, string> NewBody(string key, string value)
        {
            Dictionary<string, string> body = new Dictionary<string, string>();
            body[key] = value ?? string.Empty;
            return body;
        }

        // POST 공통: JSON 본문을 보내고 {success,message} 응답을 ActionResult로 변환한다.
        private static ActionResult Post(string path, Dictionary<string, string> body)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            using (WebClient client = new TimedWebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string response = client.UploadString(
                        apiBaseUrl + path, "POST", serializer.Serialize(body));

                Dictionary<string, object> map =
                        serializer.Deserialize<Dictionary<string, object>>(response);

                ActionResult result = new ActionResult();
                result.Success = map != null && map.ContainsKey("success")
                        && Convert.ToBoolean(map["success"]);
                result.Message = map != null && map.ContainsKey("message") && map["message"] != null
                        ? map["message"].ToString()
                        : string.Empty;
                return result;
            }
        }
    }
}
