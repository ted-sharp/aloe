// <copyright file="PatientServiceTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using Aloe.Apps.Medock.MdPatientLib.Data;
using Aloe.Apps.Medock.MdPatientLib.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdPatientLib.Tests;

/// <summary>
/// <see cref="PatientService"/> のテスト。
/// </summary>
public class PatientServiceTests : IDisposable
{
    private readonly MdPatientDbContext _db;
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        var options = new DbContextOptionsBuilder<MdPatientDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new MdPatientDbContext(options);
        _sut = new PatientService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task CreateAsync_新規患者を作成できる()
    {
        var request = new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PrimaryOrgId = Guid.NewGuid(),
            PtCode = "P001",
            PtName = "山田 太郎",
            PtNameKatakana = "ヤマダ タロウ",
            BirthDate = "1990-01-15",
            SexCode = 1,
        };

        var result = await _sut.CreateAsync(request);

        result.PtId.Should().NotBeEmpty();
        result.PtCode.Should().Be("P001");
        result.PtName.Should().Be("山田 太郎");
        result.BirthDate.Should().Be("1990-01-15");
        result.SexCode.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_作成した患者を取得できる()
    {
        var created = await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P002",
            PtName = "佐藤 花子",
            PtNameKatakana = "サトウ ハナコ",
            BirthDate = "1985-06-20",
            SexCode = 2,
        });

        var result = await _sut.GetByIdAsync(created.PtId);

        result.Should().NotBeNull();
        result!.PtCode.Should().Be("P002");
        result.PtName.Should().Be("佐藤 花子");
    }

    [Fact]
    public async Task GetByIdAsync_存在しないIDはnullを返す()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_キーワードで検索できる()
    {
        await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P010",
            PtName = "田中 一郎",
            PtNameKatakana = "タナカ イチロウ",
            BirthDate = "1970-03-10",
            SexCode = 1,
        });
        await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P011",
            PtName = "鈴木 次郎",
            PtNameKatakana = "スズキ ジロウ",
            BirthDate = "1980-07-25",
            SexCode = 1,
        });

        var result = await _sut.SearchAsync(new SearchPatientsRequest { Keyword = "田中" });

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].PtName.Should().Be("田中 一郎");
    }

    [Fact]
    public async Task SearchAsync_空キーワードで全件取得できる()
    {
        await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P020",
            PtName = "テスト 患者A",
            PtNameKatakana = "テスト カンジャA",
            BirthDate = "2000-01-01",
            SexCode = 1,
        });
        await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P021",
            PtName = "テスト 患者B",
            PtNameKatakana = "テスト カンジャB",
            BirthDate = "2000-02-02",
            SexCode = 2,
        });

        var result = await _sut.SearchAsync(new SearchPatientsRequest());

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_患者情報を更新できる()
    {
        var created = await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P030",
            PtName = "更新前 名前",
            PtNameKatakana = "コウシンマエ ナマエ",
            BirthDate = "1995-05-05",
            SexCode = 1,
        });

        var result = await _sut.UpdateAsync(new UpdatePatientRequest
        {
            PtId = created.PtId,
            PtCode = "P030",
            PtName = "更新後 名前",
            PtNameKatakana = "コウシンゴ ナマエ",
            BirthDate = "1995-05-05",
            SexCode = 1,
        });

        result.Should().NotBeNull();
        result!.PtName.Should().Be("更新後 名前");
    }

    [Fact]
    public async Task UpdateAsync_存在しないIDはnullを返す()
    {
        var result = await _sut.UpdateAsync(new UpdatePatientRequest
        {
            PtId = Guid.NewGuid(),
            PtCode = "X",
            PtName = "X",
            PtNameKatakana = "X",
            BirthDate = "2000-01-01",
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_論理削除できる()
    {
        var created = await _sut.CreateAsync(new CreatePatientRequest
        {
            FacilityId = Guid.NewGuid(),
            PtCode = "P040",
            PtName = "削除 テスト",
            PtNameKatakana = "サクジョ テスト",
            BirthDate = "1990-12-31",
            SexCode = 2,
        });

        var result = await _sut.DeleteAsync(created.PtId);
        result.Should().BeTrue();

        // QueryFilter で IsDeleted=true は除外される（InMemory では QueryFilter が効くか確認）
        var afterDelete = await _sut.GetByIdAsync(created.PtId);
        afterDelete.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_存在しないIDはfalseを返す()
    {
        var result = await _sut.DeleteAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }
}
