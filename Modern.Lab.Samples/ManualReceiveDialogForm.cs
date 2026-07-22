using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Data;
using Modern.Lab.Samples.Services;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Manual Receive 다이얼로그 — 조회(발송 통보/의뢰 인터페이스)에 나오지
    /// 않는 아이템을 강제 수신 처리한다. 동선은 2단계다:
    ///
    /// 1) 발송 공장 선택 → Lot ID 입력 → **Check**: 서버에서 그 Lot의
    ///    웨이퍼(Unit) 정보를 조회해 그리드에 보여준다 — 실물과 전산이
    ///    맞는지 눈으로 확인하는 단계다. 확인되면 Receive가 활성화된다.
    /// 2) **Receive**: 발송 공장과 함께 강제 수신 전문을 호출한다 — 서버가
    ///    체크(중복 수신 거부)와 처리를 한 번에 수행하고, 실패 사유는
    ///    다이얼로그 안 배지에 남는다(교정 후 재시도). 입력이 바뀌면 확인이
    ///    무효가 되어 다시 Check해야 한다.
    ///
    /// 성공하면 다이얼로그를 닫고 부모 폼이 그 아이템으로 재조회한다.
    ///
    /// ★ 회사 환경 교체 지점 — 서버 호출은 전부 이 다이얼로그 하단 "서버
    ///   호출" 구획에 모여 있다. GetUnits(웨이퍼 확인)/ManualReceive(수신
    ///   전문)/GetSendFacilities(공장 코드)의 본문을 회사 인터페이스로 바꾸면
    ///   나머지 코드는 그대로 둔다.
    /// </summary>
    public partial class ManualReceiveDialogForm : Form
    {
        // 결과 배지 색 — 실패(빨강 틴트)/입력 안내(호박 틴트).
        private const string errorColor = "#FEE2E2";
        private const string warningColor = "#FEF3C7";

        // Check로 확인이 끝난 Lot ID — 입력이 바뀌면 무효가 된다(다시 Check).
        private string checkedLotId;

        /// <summary>강제 수신된 Item ID (DialogResult가 OK일 때만 유효).</summary>
        public string ReceivedItemId { get; private set; }

        public ManualReceiveDialogForm()
        {
            this.InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            // 웨이퍼 확인 그리드: Unit 식별 정보 위주(메인 폼 Unit 리스트와 동일).
            this.gridUnits.ConfigureColumns(
                new ModernDataGridColumn("UNIT_ID"),
                new ModernDataGridColumn("SUB_TYP", "Type") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("STAT_TYP") { TextAlignment = GridTextAlignment.Center },
                new ModernDataGridColumn("EVENT_TM") { TextAlignment = GridTextAlignment.Center });

            // 발송 공장 콤보 — 서버에서 공장 코드 목록을 받아 채운다.
            try
            {
                this.cboSendFac.DataSource = GetSendFacilities();
            }
            catch (Exception)
            {
                // 공장 코드 조회 실패 시 빈 콤보로 둔다 — 수신 시 서버가 다시 검증한다.
                this.cboSendFac.DataSource = new string[0];
            }

            // 발송 공장 선택이 첫 입력이다 — 포커스도 콤보부터.
            this.cboSendFac.Focus();
        }

        // Check: 입력한 Lot의 웨이퍼(Unit) 정보를 조회해 그리드에 보여준다 —
        // 확인되면 Receive가 활성화된다. 없는 Lot이면 사유를 배지에 남긴다.
        private void OnCheckClick(object sender, EventArgs e)
        {
            string lotId = this.txtLotId.Text.Trim();

            if (lotId.Length == 0)
            {
                this.ShowResult("Enter a lot ID.", warningColor);
                this.txtLotId.Focus();
                return;
            }

            DataTable units;

            try
            {
                units = GetUnits(lotId);
            }
            catch (Exception ex)
            {
                this.ShowResult("Server call failed: " + ex.Message, errorColor);
                return;
            }

            if (units.Rows.Count == 0)
            {
                this.gridUnits.DataSource = null;
                this.checkedLotId = null;
                this.btnReceive.Enabled = false;
                this.ShowResult("No wafer found for " + lotId + ".", warningColor);
                return;
            }

            // 확인 완료 — 웨이퍼 정보를 뿌리고 Receive를 연다.
            this.gridUnits.DataSource = units;
            this.checkedLotId = lotId;
            this.btnReceive.Enabled = true;
            this.HideResult();
        }

        // Receive: Check로 확인된 Lot만 강제 수신한다 — 서버가 중복 체크 +
        // 처리를 한 번에 한다. 실패 사유는 배지에 남기고 다이얼로그를 유지한다.
        private void OnReceiveClick(object sender, EventArgs e)
        {
            string lotId = this.txtLotId.Text.Trim();

            if (this.checkedLotId == null || this.checkedLotId != lotId)
            {
                this.ShowResult("Check the lot ID first.", warningColor);
                return;
            }

            string sendFac = this.cboSendFac.SelectedValue as string;

            ActionResult result;

            try
            {
                result = ManualReceive(lotId, sendFac);
            }
            catch (Exception ex)
            {
                this.ShowResult("Server call failed: " + ex.Message, errorColor);
                return;
            }

            if (!result.Success)
            {
                this.ShowResult(result.Message, errorColor);
                return;
            }

            this.ReceivedItemId = lotId.ToUpperInvariant();
            this.DialogResult = DialogResult.OK;
        }

        // 입력이 바뀌면 이전 확인/결과를 무효화한다 — 낡은 웨이퍼 목록이 새
        // 입력에 대한 확인처럼 보이지 않게 Receive를 잠그고 다시 Check를 받는다.
        private void OnInputChanged(object sender, EventArgs e)
        {
            this.checkedLotId = null;
            this.btnReceive.Enabled = false;
            this.HideResult();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        // 다이얼로그 관례 키: Enter = 확인 전이면 Check, 확인 후면 Receive.
        // Esc = 닫기. (ModernButton은 IButtonControl이 아니라 폼의
        // AcceptButton/CancelButton에 지정할 수 없어 키를 직접 처리한다.)
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.btnReceive.Enabled)
                {
                    this.OnReceiveClick(this.btnReceive, EventArgs.Empty);
                }
                else
                {
                    this.OnCheckClick(this.btnCheck, EventArgs.Empty);
                }

                return true;
            }

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void ShowResult(string message, string color)
        {
            this.badgeResult.Text = message;
            this.badgeResult.Color = color;
            this.badgeResult.Visible = true;
        }

        private void HideResult()
        {
            this.badgeResult.Visible = false;
        }

        // ===== 서버 호출 (★ 회사 환경 교체 지점) =====
        // 홈 환경은 modernlab-api(REST)를 호출한다. 회사 적용 시 아래 세 메서드
        // 본문만 회사 인터페이스 호출로 바꾸면 나머지 코드는 그대로 둔다.

        private const string apiBaseUrl = "http://localhost:8080";
        private const int apiTimeoutMs = 5000;

        /// <summary>서버 처리 응답 — 성공 여부와 실패 사유.</summary>
        private sealed class ActionResult
        {
            internal bool Success;
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

        /// <summary>발송 공장 코드 목록 — 공장 콤보 원천.
        /// ★ 회사 적용 시 공장 코드 마스터 조회로 교체한다.</summary>
        private static string[] GetSendFacilities()
        {
            using (WebClient client = new TimedWebClient())
            {
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString(apiBaseUrl + "/api/pending/send-facilities");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Deserialize<string[]>(json) ?? new string[0];
            }
        }

        /// <summary>Lot의 웨이퍼(Unit) 목록 — Check 단계의 확인 조회.
        /// ★ 회사 적용 시 Lot 확인 전문(웨이퍼 목록 조회)으로 교체한다.</summary>
        private static DataTable GetUnits(string lotId)
        {
            using (WebClient client = new TimedWebClient())
            {
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString(
                        apiBaseUrl + "/api/items/units?itemId=" + Uri.EscapeDataString(lotId ?? string.Empty));
                return JsonTableConverter.ToDataTable(json);
            }
        }

        /// <summary>강제 수신 단일 전문 — 서버가 체크(중복 수신 거부)와 처리를
        /// 한 번에 수행하고 실패면 사유를 돌려준다. ★ 회사 적용 시 수신 전문
        /// 호출로 바꾼다.</summary>
        private static ActionResult ManualReceive(string itemId, string sendFac)
        {
            Dictionary<string, string> body = new Dictionary<string, string>();
            body["itemId"] = itemId ?? string.Empty;
            body["sendFac"] = sendFac ?? string.Empty;
            return Post("/api/pending/manual-receive", body);
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
