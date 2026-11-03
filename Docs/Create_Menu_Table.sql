-- =============================================
-- Menu 테이블 생성 및 기본 데이터 입력
-- 작성일: 2026-11-03
-- =============================================

-- Menu 테이블 생성
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Menu')
BEGIN
    CREATE TABLE Menu (
        MenuId NVARCHAR(50) PRIMARY KEY,
        ParentMenuId NVARCHAR(50) NULL,
        MenuName NVARCHAR(100) NOT NULL,
        FormName NVARCHAR(100) NULL,          -- 폼 태그 (Login, UserManage 등)
        MenuOrder INT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        CreatedBy NVARCHAR(50),
        CreatedAt DATETIME DEFAULT GETDATE(),
        ModifiedBy NVARCHAR(50),
        ModifiedAt DATETIME
    );
    PRINT 'Menu 테이블 생성 완료';
END
GO

-- 기존 데이터 삭제 (필요시)
-- DELETE FROM Menu;

-- 기본 메뉴 데이터 입력
PRINT '메뉴 데이터 입력 시작...';

-- 시스템 관리 (상위)
IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'SYS')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('SYS', NULL, '시스템 관리', NULL, 1, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'SYS_LOGIN')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('SYS_LOGIN', 'SYS', '로그인', 'Login', 11, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'SYS_USER')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('SYS_USER', 'SYS', '사용자 관리', 'UserManage', 12, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'SYS_AUTH')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('SYS_AUTH', 'SYS', '권한 관리', 'AuthManage', 13, 1, 'SYSTEM');

-- 기준정보 (상위)
IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'MST')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('MST', NULL, '기준정보', NULL, 2, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'MST_CODE')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('MST_CODE', 'MST', '공통코드 관리', 'CommonCode', 21, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'MST_CUST')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('MST_CUST', 'MST', '거래처 관리', 'Customer', 22, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'MST_ITEM')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('MST_ITEM', 'MST', '품목 관리', 'Item', 23, 1, 'SYSTEM');

-- 업무 (상위)
IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'WRK')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('WRK', NULL, '업무', NULL, 3, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'WRK_ORD')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('WRK_ORD', 'WRK', '주문 관리', 'Order', 31, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'WRK_INV')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('WRK_INV', 'WRK', '재고 관리', 'Inventory', 32, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'WRK_SHP')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('WRK_SHP', 'WRK', '출하 관리', 'Shipment', 33, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'WRK_RECV')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('WRK_RECV', 'WRK', '미수금 관리', 'Receivable', 34, 1, 'SYSTEM');

-- 프로그램 (상위)
IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'PRG')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('PRG', NULL, '프로그램', NULL, 9, 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE MenuId = 'PRG_EXIT')
    INSERT INTO Menu (MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive, CreatedBy)
    VALUES ('PRG_EXIT', 'PRG', '종료', 'Exit', 91, 1, 'SYSTEM');

PRINT '메뉴 데이터 입력 완료';

-- 결과 확인
SELECT MenuId, ParentMenuId, MenuName, FormName, MenuOrder, IsActive
FROM Menu
ORDER BY MenuOrder, MenuId;
GO
