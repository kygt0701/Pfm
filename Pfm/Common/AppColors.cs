using System.Drawing;

namespace Pfm.Common
{
    /// <summary>
    /// 애플리케이션 색상 테마 설정
    /// 뉴트럴 그레이 + 포인트 컬러 (SaaS 스타일)
    /// </summary>
    public static class AppColors
    {
        #region 기본 배경색 (Neutral Gray)
        /// <summary>메인 배경색 - 밝은 회색</summary>
        public static Color BackgroundMain => Color.FromArgb(248, 249, 250);

        /// <summary>패널/카드 배경색 - 흰색</summary>
        public static Color BackgroundPanel => Color.FromArgb(255, 255, 255);

        /// <summary>사이드바 배경색 - 진한 회색</summary>
        public static Color BackgroundSidebar => Color.FromArgb(52, 58, 64);

        /// <summary>헤더 배경색</summary>
        public static Color BackgroundHeader => Color.FromArgb(73, 80, 87);
        #endregion

        #region 텍스트 색상
        /// <summary>기본 텍스트 - 진한 회색</summary>
        public static Color TextPrimary => Color.FromArgb(33, 37, 41);

        /// <summary>보조 텍스트 - 중간 회색</summary>
        public static Color TextSecondary => Color.FromArgb(108, 117, 125);

        /// <summary>비활성 텍스트 - 연한 회색</summary>
        public static Color TextDisabled => Color.FromArgb(173, 181, 189);

        /// <summary>밝은 배경 위 텍스트 - 흰색</summary>
        public static Color TextLight => Color.FromArgb(255, 255, 255);
        #endregion

        #region 테두리 색상
        /// <summary>기본 테두리</summary>
        public static Color BorderDefault => Color.FromArgb(222, 226, 230);

        /// <summary>포커스 테두리</summary>
        public static Color BorderFocus => Color.FromArgb(0, 150, 136);
        #endregion

        #region 포인트 컬러 (Primary - 청록/틸)
        /// <summary>주요 액션 버튼 (저장, 확정)</summary>
        public static Color Primary => Color.FromArgb(0, 150, 136);

        /// <summary>주요 색상 호버</summary>
        public static Color PrimaryHover => Color.FromArgb(0, 137, 123);

        /// <summary>주요 색상 연한 버전 (선택 행 배경)</summary>
        public static Color PrimaryLight => Color.FromArgb(224, 242, 241);

        /// <summary>주요 색상 매우 연한 버전</summary>
        public static Color PrimaryLighter => Color.FromArgb(237, 247, 246);
        #endregion

        #region 보조 컬러 (Secondary - 파랑)
        /// <summary>보조 액션</summary>
        public static Color Secondary => Color.FromArgb(33, 150, 243);

        /// <summary>보조 색상 호버</summary>
        public static Color SecondaryHover => Color.FromArgb(25, 118, 210);

        /// <summary>보조 색상 연한 버전</summary>
        public static Color SecondaryLight => Color.FromArgb(227, 242, 253);
        #endregion

        #region 상태 색상 - 경고 (Amber/Warning)
        /// <summary>경고 - 미수 연체, 여신 초과</summary>
        public static Color Warning => Color.FromArgb(255, 152, 0);

        /// <summary>경고 호버</summary>
        public static Color WarningHover => Color.FromArgb(245, 124, 0);

        /// <summary>경고 연한 배경</summary>
        public static Color WarningLight => Color.FromArgb(255, 243, 224);

        /// <summary>경고 텍스트</summary>
        public static Color WarningText => Color.FromArgb(230, 81, 0);
        #endregion

        #region 상태 색상 - 오류 (Red/Error)
        /// <summary>오류</summary>
        public static Color Error => Color.FromArgb(244, 67, 54);

        /// <summary>오류 호버</summary>
        public static Color ErrorHover => Color.FromArgb(211, 47, 47);

        /// <summary>오류 연한 배경</summary>
        public static Color ErrorLight => Color.FromArgb(255, 235, 238);

        /// <summary>오류 텍스트</summary>
        public static Color ErrorText => Color.FromArgb(198, 40, 40);
        #endregion

        #region 상태 색상 - 성공 (Green/Success)
        /// <summary>성공</summary>
        public static Color Success => Color.FromArgb(76, 175, 80);

        /// <summary>성공 호버</summary>
        public static Color SuccessHover => Color.FromArgb(56, 142, 60);

        /// <summary>성공 연한 배경</summary>
        public static Color SuccessLight => Color.FromArgb(232, 245, 233);

        /// <summary>성공 텍스트</summary>
        public static Color SuccessText => Color.FromArgb(46, 125, 50);
        #endregion

        #region 상태 색상 - 정보 (Blue/Info)
        /// <summary>정보</summary>
        public static Color Info => Color.FromArgb(33, 150, 243);

        /// <summary>정보 연한 배경</summary>
        public static Color InfoLight => Color.FromArgb(227, 242, 253);
        #endregion

        #region 그리드 색상
        /// <summary>그리드 헤더 배경 - 진한 회색</summary>
        public static Color GridHeader => Color.FromArgb(73, 80, 87);

        /// <summary>그리드 헤더 텍스트 - 흰색</summary>
        public static Color GridHeaderText => Color.FromArgb(255, 255, 255);

        /// <summary>그리드 짝수 행</summary>
        public static Color GridRowEven => Color.FromArgb(255, 255, 255);

        /// <summary>그리드 홀수 행</summary>
        public static Color GridRowOdd => Color.FromArgb(248, 249, 250);

        /// <summary>그리드 선택 행</summary>
        public static Color GridRowSelected => PrimaryLight;

        /// <summary>그리드 호버 행</summary>
        public static Color GridRowHover => Color.FromArgb(243, 244, 246);
        #endregion

        #region 버튼 색상
        /// <summary>기본 버튼 배경</summary>
        public static Color ButtonDefault => Color.FromArgb(255, 255, 255);

        /// <summary>기본 버튼 테두리</summary>
        public static Color ButtonDefaultBorder => Color.FromArgb(206, 212, 218);

        /// <summary>취소 버튼</summary>
        public static Color ButtonCancel => Color.FromArgb(108, 117, 125);
        #endregion
    }
}
