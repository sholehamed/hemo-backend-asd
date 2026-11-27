# ST-02 tests
- dotnet test tests/Hemo.Admission.Tests --filter "Category!=SqlServer"
- dotnet test tests/Hemo.Admission.Tests --filter "Category=SqlServer" (requires HEMO_TEST_SQL pointing to a dedicated disposable SQL Server; random test database is created and deleted).
- Tests cover rule validation, EF model, SQL migration script and actual migration on a clean database. They do not exercise real company authentication or a live HIS contract.
- No existing company project or service has been modified to enable these tests.
