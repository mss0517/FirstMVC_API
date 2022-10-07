mPHR 資料查詢練習
===

## Example

以建立時間到序，查詢前十筆有效機構名稱：
使用 Table ：`Organizations`

```
Select Top 10 Name 
From dbo.Organizations
Order by CreatedAt desc
```


## Questions

### **1. 查詢所有有效帳號的「GID」**
使用 Table ：`Accounts`

EF Core
```
await _context.Accounts.Where(x => x.Status == AccountStatus.Available).Select(x => x.Gid)
.ToListAsync();
```
T-SQL
```
SELECT
	Gid 
FROM
	Accounts 
WHERE
	Status = '0';
```

### **2. 顯示第 11~20 筆「最新建立」的有效帳號「GID」和「帳號」（分頁機制）**
使用 Table ：`Accounts`

EF Core
```
await _context.Accounts.OrderByDescending(x => x.CreatedAt)
.Select(x => new { x.Gid, x.UserName })
.Skip(10)
.Take(10)
.ToListAsync();
```
T-SQL
```
SELECT
	Gid,
	UserName 
FROM
	( SELECT *, ROW_NUMBER ( ) OVER ( ORDER BY CreatedAt DESC ) AS ROWNUM FROM Accounts ) result 
WHERE
	result.ROWNUM > 10 
	AND result.ROWNUM < 21;
```


### **3. 查詢有授權給「先進醫資展示服務機構」機構的有效帳號「GID」**
使用 Table ：`Organizations` `OrganizeAuthorizations`

EF Core
```
await _context.Organizations.Where(x => x.Name == "先進醫資展示服務機構")
.Join(_context.OrganizeAuthorizations, p => p.Id, s => s.OrganizationId, (p, s) => new { s.AccountGid, s.RemovedAt })
.Where(x => x.RemovedAt == null)
.Select(x => x.AccountGid)
.ToListAsync();
```
T-SQL
```
SELECT
	AccountGid 
FROM
	OrganizeAuthorizations 
WHERE
	OrganizeAuthorizations.RemovedAt IS NULL 
	AND ( SELECT Id FROM Organizations WHERE Name = '先進醫資展示服務機構' ) = OrganizeAuthorizations.Id;
```


### 4. **查詢「先進醫資展示服務機構」有使用的照護站待機影片「檔案網址」**
使用 Table ：`Organizations` `OrganizationKioskVideos` `Files`

EF Core
```
await _context.Organizations.Where(x => x.Name == "先進醫資展示服務機構")
.Join(_context.OrganizeAuthorizations, p => p.Id, s => s.OrganizationId, (p, s) => new { s.AccountGid, s.RemovedAt })
.Where(x => x.RemovedAt == null)
.Select(x => x.AccountGid)
.ToListAsync();
```
T-SQL
```
SELECT
	Files.FileUrl 
FROM
	Files 
WHERE
	Id IN ( SELECT OrganizationKioskVideos.FileId FROM OrganizationKioskVideos WHERE ( SELECT Id FROM Organizations WHERE Name = '先進醫資展示服務機構' ) = OrganizationKioskVideos.OrganizationId );
```


### **5. 查詢「先進醫資展示服務機構」的所有照護站「機台ID」**
使用 Table ：`Organizations` `Machines`

EF Core
```
await _context.Organizations.Where(x => x.Name == "先進醫資展示服務機構")
.Join(_context.Machines, p => p.Id, s => s.OrganizationId, (p, s) => s.Id)
.ToListAsync();
```
T-SQL
```
SELECT
	Machines.Id 
FROM
	Machines 
WHERE
	( SELECT Id FROM Organizations WHERE Name = '先進醫資展示服務機構' ) = Machines.OrganizationId;
```


### **6. 查詢「先進醫資展示服務機構」機構下的有效「共照群組名稱」，顯示第 21~30 筆**
使用 Table ：`Organizations` `PatientGroup`

EF Core
```
await _context.Organizations.Where(x => x.Name == "先進醫資展示服務機構")
.Join(_context.PatientGroups, p => p.Id, s => s.OrganizationId, (p, s) => new { s.Name, s.Status })
.Where(x => x.Status == PatientGroupStatus.Available)
.Skip(20)
.Take(10)
.Select(x => x.Name)
.ToListAsync();
```
T-SQL
```
SELECT
	Name 
FROM
	(
	SELECT
		*,
		ROW_NUMBER () OVER ( ORDER BY Id ) AS ROWNUM 
	FROM
		PatientGroup 
	WHERE
		( SELECT Id FROM Organizations WHERE Name = '先進醫資展示服務機構' ) = PatientGroup.OrganizationId 
		AND
		PatientGroup.Status = 0
	) result 
WHERE
	result.ROWNUM > 20 
	AND result.ROWNUM < 31;
```


### **7. 查詢「先進醫資展示服務機構」機構下，入群人數最多的「共照群組名稱」和「人數」**
使用 Table ：`Organizations` `PatientGroup` `PatientGroupDetails`

EF Core
```
(await _context.Organizations.Where(x => x.Name == "先進醫資展示服務機構")
.Join(_context.PatientGroups, p => p.Id, s => s.OrganizationId, (p, s) => new { s.Id, s.Name })
.Join(_context.PatientGroupDetails, p => p.Id, s => s.PatientGroupId, (p, s) => p.Name)
.ToListAsync())
.GroupBy(x => x)
.Select(x => new { name = x.Key, count = x.Count() })
.OrderByDescending(x => x.count)
.Take(1);
```
T-SQL
```
SELECT TOP(1)
	result.Id,
	result.Name,
	COUNT ( PatientGroupDetails.PatientGroupId ) AS 'Count' 
FROM
	( SELECT Id, Name FROM PatientGroup WHERE ( SELECT Id FROM Organizations WHERE Organizations.Name = '先進醫資展示服務機構' ) = PatientGroup.OrganizationId ) result,
	PatientGroupDetails 
WHERE
	PatientGroupDetails.PatientGroupId = result.Id 
GROUP BY
	result.Id,
	result.Name 
ORDER BY
	'Count' DESC
```


### **8. 查詢「先進洗腎室關懷群」共照群組內所有成員的「手機號碼」，如果沒有則留空**
使用 Table ：`PatientGroup` `PatientGroupDetails` `AccountInformations`

EF Core
```
await _context.PatientGroups.Where(x => x.Name == "先進洗腎室關懷群")
.Join(_context.PatientGroupDetails, p => p.Id, s => s.PatientGroupId, (p, s) => s.AccountGid)
.Join(_context.Accounts, p => p, s => s.Gid, (p, s) => new { s.Gid, s.UserName })
.Join(_context.AccountInformations, p => p.Gid, s => s.AccountGid, (p, s) => new { p.UserName, s.Mobile })
.ToListAsync();
```
T-SQL
```
SELECT
	UserName,
	Mobile 
FROM
	Accounts,
	AccountInformations 
WHERE
	Accounts.Gid IN ( SELECT PatientGroupDetails.AccountGid FROM PatientGroupDetails WHERE PatientGroupDetails.PatientGroupId = ( SELECT Id FROM PatientGroup WHERE PatientGroup.Name = '先進洗腎室關懷群' ) )
	AND
	Accounts.Gid = AccountInformations .AccountGid;
```


### **9. 查詢帳號名稱為「路中廟社區時間銀行」的權限有哪些，列出「權限名稱」**
使用 Table ：`Accounts` `AccountRoles` `Roles`

EF Core
```
await _context.Accounts.Where(x => x.UserName == "路中廟社區時間銀行")
.Join(_context.AccountRoles, p => p.Gid, s => s.AccountGid, (p, s) => s.RoleId)
.Join(_context.Roles, p => p, s => s.Id, (p, s) => s.Name)
.ToListAsync();
```
T-SQL
```
SELECT
	Roles.Name 
FROM
	Roles 
WHERE
	Roles.Id IN ( SELECT AccountRoles.RoleId FROM AccountRoles WHERE AccountRoles.AccountGid = ( SELECT Gid FROM Accounts WHERE UserName = '路中廟社區時間銀行' ) )
```

### **10. 檢查GID（86f655dd-844f-4ae2-bb54-09281a85711e）的帳號**
    1. 是否有驗證過手機號碼
    2. 是否有驗證過身份證字號
    3. 是否有驗證過電子信箱

使用 Table ：`Accounts` `AccountInformations` `AccountCertificates` 

EF Core
```
await _context.AccountCertificates.Where(x => x.AccountGid == Guid.Parse("86f655dd-844f-4ae2-bb54-09281a85711e"))
.GroupBy(x => x.AccountGid)
.Select(x => new
{
  Gid = x.Key,
  Mobile = x.Any(c => c.CertiType == AccountCertificateType.Mobile) ? "手機已驗證" : "手機未驗證",
  Email = x.Any(c => c.CertiType == AccountCertificateType.Email) ? "電子郵件已驗證" : "電子郵件未驗證",
  IdNo = x.Any(c => c.CertiType == AccountCertificateType.IdentityNo) ? "身分證已驗證" : "身分證未驗證"
})
.ToListAsync();
```
T-SQL
```
SELECT
CASE
		
	WHEN
		( COUNT ( Emailv.Id ) ) >0 THEN
			'已驗證' ELSE '未驗證' 
			END AS 'Email驗證',
	CASE
			
			WHEN ( COUNT ( Phonev.Id ) ) >0 THEN
			'已驗證' ELSE '未驗證' 
		END AS 'Phone驗證',
	CASE
			
			WHEN IdNO.IdentityNo IS NULL THEN
			'未驗證' ELSE '已驗證' 
		END AS '身分證驗證' 
	FROM
		( SELECT Id, CertiType FROM AccountCertificates WHERE AccountCertificates.AccountGid = '86f655dd-844f-4ae2-bb54-09281a85711e' AND AccountCertificates.CertiType = 20 ) Emailv,
		( SELECT Id, CertiType FROM AccountCertificates WHERE AccountCertificates.AccountGid = '86f655dd-844f-4ae2-bb54-09281a85711e' AND AccountCertificates.CertiType = 10 ) Phonev,
		( SELECT AccountInformations.IdentityNo FROM AccountInformations WHERE AccountInformations.AccountGid = '86f655dd-844f-4ae2-bb54-09281a85711e' ) IdNO 
	GROUP BY
		Emailv.Id,
	Phonev.Id,
	IdNO.IdentityNo
```
