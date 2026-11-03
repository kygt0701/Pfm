-- =============================================
-- 메뉴 ID 수정 스크립트
-- 작성일: 2026-11-03
-- 설명: 폼의 MenuId와 DB MenuId 일치시키기
-- =============================================

-- 트랜잭션 시작
BEGIN TRANSACTION;

-- =============================================
-- 1. Menu 테이블 MenuId 수정
-- =============================================
PRINT '1. Menu 테이블 MenuId 수정 시작...';

-- 주문 관리: WRK_ORD -> ORD_ORDER
UPDATE Menu SET MenuId = 'ORD_ORDER' WHERE MenuId = 'WRK_ORD';

-- 재고 관리: WRK_INV -> INV_STOCK
UPDATE Menu SET MenuId = 'INV_STOCK' WHERE MenuId = 'WRK_INV';

-- 출하 관리: WRK_SHP -> SHP_SHIP
UPDATE Menu SET MenuId = 'SHP_SHIP' WHERE MenuId = 'WRK_SHP';

-- 미수금 관리: WRK_RCV -> ACC_RECV
UPDATE Menu SET MenuId = 'ACC_RECV' WHERE MenuId = 'WRK_RCV';

PRINT '   Menu 테이블 수정 완료';

-- =============================================
-- 2. MenuPermission 테이블 MenuId 수정
-- =============================================
PRINT '2. MenuPermission 테이블 MenuId 수정 시작...';

-- 주문 관리
UPDATE MenuPermission SET MenuId = 'ORD_ORDER' WHERE MenuId = 'WRK_ORD';

-- 재고 관리
UPDATE MenuPermission SET MenuId = 'INV_STOCK' WHERE MenuId = 'WRK_INV';

-- 출하 관리
UPDATE MenuPermission SET MenuId = 'SHP_SHIP' WHERE MenuId = 'WRK_SHP';

-- 미수금 관리
UPDATE MenuPermission SET MenuId = 'ACC_RECV' WHERE MenuId = 'WRK_RCV';

PRINT '   MenuPermission 테이블 수정 완료';

-- =============================================
-- 3. 결과 확인
-- =============================================
PRINT '3. 수정 결과 확인...';

SELECT 'Menu 테이블' AS TableName, MenuId, MenuName 
FROM Menu 
WHERE MenuId IN ('ORD_ORDER', 'INV_STOCK', 'SHP_SHIP', 'ACC_RECV', 
                 'SYS_LOGIN', 'SYS_USER', 'SYS_AUTH', 'MST_CODE', 'MST_CUST', 'MST_ITEM')
ORDER BY MenuId;

SELECT 'MenuPermission 테이블' AS TableName, RoleId, MenuId, CanRead, CanCreate, CanSave, CanDelete
FROM MenuPermission
WHERE MenuId IN ('ORD_ORDER', 'INV_STOCK', 'SHP_SHIP', 'ACC_RECV',
                 'SYS_LOGIN', 'SYS_USER', 'SYS_AUTH', 'MST_CODE', 'MST_CUST', 'MST_ITEM')
ORDER BY RoleId, MenuId;

-- =============================================
-- 4. 커밋 (문제 없으면 주석 해제)
-- =============================================
-- 확인 후 아래 COMMIT 실행
COMMIT TRANSACTION;
-- 문제 발생 시 ROLLBACK
-- ROLLBACK TRANSACTION;

PRINT '스크립트 실행 완료!';
GO
