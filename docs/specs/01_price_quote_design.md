# 01. PriceQuote 도메인 설계

## 목표

- 활성 RatePlan과 투숙 기간의 DailyRate를 하나의 만료 가능한 가격 제안으로 결합한다.
- 견적 발급 이후 DailyRate나 CancellationPolicy가 변경되어도 유효기간 동안 제시한 가격과 취소조건을 보존한다.
- 일별 가격 완전성, RatePlan 및 통화 일관성, 총액 계산을 PriceQuote Aggregate 안에서 보호한다.
- 향후 Reservation Price Snapshot이 합의 가격과 취소 위약금 계산 자료를 안전하게 인수할 기반을 마련한다.

## 적용 범위

- 도메인 용어: `CONTEXT.md`
- Pricing 도메인: `backend/src/HotelBooking.Modules.Pricing/Domain/PriceQuotes`
- 단위 테스트: `backend/tests/HotelBooking.UnitTests/Pricing/PriceQuotes`
- 비범위: EF Core 매핑, PostgreSQL 마이그레이션, PriceQuote 저장소와 애플리케이션 유스케이스, Property의 RoomType 수용 가능성 조회, 호텔 현지 날짜 검증, HTTP API, Reservation 및 Reservation Price Snapshot, Inventory 조회와 Hold

## 사전 조건

- RatePlan은 호텔, Room Type, 판매 통화, CancellationPolicy를 보유한다.
- DailyRate는 하나의 RatePlan과 하나의 점유일에 대한 `Money` 가격을 보유한다.
- `StayDateRange`는 체크인을 포함하고 체크아웃을 제외한 점유일을 제공한다.
- `Money`는 같은 통화끼리만 합산할 수 있다.
- 첫 번째 예약 흐름은 한 PriceQuote에서 객실 한 개만 견적한다.

## 단계별 적용

## 01.1 견적 언어와 보장 경계 확정

- 객실 수용 한도인 Property의 `Occupancy`와 실제 요청 인원인 `Requested Occupancy`를 구분한다.
- PriceQuote는 명시된 만료 시각 전까지 일별 가격과 취소조건을 보장한다.
- PriceQuote는 Reservation, Inventory Hold 또는 현재 판매 가능성에 대한 보장이 아니다.
- 견적 발급 뒤 DailyRate가 바뀌어도 유효한 기존 견적 가격은 바뀌지 않는다.
- 견적 발급 뒤 Hotel, Room Type 또는 RatePlan이 판매 불가능해지면 향후 Reservation 생성 유스케이스가 현재 판매 가능성을 다시 검증한다.

## 01.2 식별자와 불변 값 구현

- `PriceQuoteId`는 `Guid` 기반의 강한 타입 식별자로 구현한다.
- `RequestedOccupancy`는 한 객실에 투숙하려는 성인과 어린이 수를 보존한다.
- RequestedOccupancy는 성인 한 명 이상과 음수가 아닌 어린이 수를 요구한다.
- `NightlyPrice`는 하나의 점유일과 해당 시점에 제시된 `Money` 가격을 보존한다.
- NightlyPrice는 PriceQuote가 검증된 DailyRate에서만 구성하고 외부 생성 인터페이스를 제공하지 않는다.

## 01.3 PriceQuote Aggregate Root 구현

- PriceQuote 생성 인터페이스는 식별자, RatePlan, StayDateRange, RequestedOccupancy, DailyRate 목록, QuotedAt, ExpiresAt을 받는다.
- RatePlan은 `Active` 상태여야 한다.
- DailyRate 목록은 투숙 기간의 모든 점유일과 정확히 일치해야 하며 누락, 중복, 기간 밖 날짜를 허용하지 않는다.
- 모든 DailyRate는 견적의 RatePlan 식별자와 판매 통화를 사용해야 한다.
- NightlyPrice는 점유일 오름차순으로 보존한다.
- TotalPrice는 외부 입력을 받지 않고 `Money.Add`로 일별 가격을 합산한다.
- CancellationPolicy는 RatePlan에서 복사하여 견적 시점의 취소조건으로 보존한다.
- QuotedAt과 ExpiresAt은 애플리케이션이 명시적으로 전달하며 ExpiresAt은 QuotedAt보다 뒤여야 한다.
- PriceQuote는 정확히 ExpiresAt부터 만료된 것으로 판단한다.
- PriceQuote는 소비 상태나 변경 동작을 갖지 않는 불변 Aggregate Root다.

## 01.4 단위 테스트 작성

- 정상적인 다박 견적이 날짜순 일별 가격, 총액, 식별자와 조건을 보존하는지 검증한다.
- 0원 DailyRate를 포함한 견적과 단일 박 견적을 검증한다.
- RequestedOccupancy의 정상값과 성인·어린이 경계를 검증한다.
- 누락된 필수 값과 잘못된 견적 유효기간을 거부하는지 검증한다.
- 비활성 RatePlan을 거부하는지 검증한다.
- DailyRate 누락, 중복, 기간 밖 날짜, 다른 RatePlan 및 다른 통화를 거부하는지 검증한다.
- 견적 만료 경계 직전과 정확한 경계 시각을 검증한다.
- 반환된 일별 가격 컬렉션을 외부에서 변경할 수 없는지 검증한다.

## 도메인 규칙

- 첫 버전의 PriceQuote는 하나의 Room Type에 속한 객실 한 개를 대상으로 한다.
- RequestedOccupancy는 가격 계산 입력이자 합의 조건이지만 Room Type의 최대 수용 가능성은 Property가 소유한다.
- TotalPrice는 모든 점유일의 NightlyPrice 합계이며 별도 입력으로 덮어쓸 수 없다.
- 모든 NightlyPrice와 TotalPrice는 RatePlan의 Hotel Selling Currency를 사용한다.
- PriceQuote는 유효기간 동안 가격과 CancellationPolicy를 보장하지만 재고와 현재 판매 가능성을 보장하지 않는다.
- DailyRate의 0원 가격은 무료 프로모션이나 보상 숙박을 위해 그대로 허용한다.
- PriceQuote는 일회용 쿠폰이 아니므로 소비 상태를 갖지 않는다. Reservation 생성 재시도의 중복 방지는 향후 Reservation 명령의 멱등성이 담당한다.

## 모듈과 시간 책임

- Pricing 도메인은 Property 구현 어셈블리를 참조하지 않는다.
- Room Type이 RequestedOccupancy를 수용할 수 있는지는 향후 애플리케이션 유스케이스가 Property 계약을 통해 검증한다.
- 향후 Reservation 생성은 PriceQuote 만료뿐 아니라 Hotel, Room Type, RatePlan의 현재 판매 가능성을 다시 검증한다.
- PriceQuote는 시스템 시계를 직접 읽지 않는다. 애플리케이션이 TimeProvider와 설정으로 QuotedAt 및 ExpiresAt을 계산한다.
- QuotedAt은 견적 발급과 만료 기준이며 체크인 과거 여부 판단에 사용하지 않는다.
- 과거 체크인은 향후 애플리케이션 유스케이스가 호텔 현지 날짜를 명시적으로 계산하여 거부한다.
- 견적 유효기간은 Inventory Hold의 10분 만료정책과 독립적이다.

## Aggregate 경계와 영속성

- PriceQuote는 독립 식별자와 유효기간을 갖고 Reservation 생성에서 참조되는 Aggregate Root다.
- RatePlan과 DailyRate Aggregate는 생성 입력으로만 사용하며 PriceQuote는 그 객체 참조를 보존하지 않는다.
- PriceQuote는 RatePlanId, 호텔 및 Room Type 식별자, RequestedOccupancy, 스냅샷된 일별 가격과 취소조건만 보존한다.
- 이번 범위에는 영속성이 없으므로 만료 전 견적 조회, 동시 예약 생성 또는 데이터베이스 제약을 증명하지 않는다.

## 장애 대응/롤백

- 입력 DailyRate가 완전하지 않거나 일관되지 않으면 PriceQuote를 생성하지 않는다.
- 합산 중 `decimal` 범위를 초과하면 `Money.Add`의 `OverflowException`을 그대로 전달하며 불완전한 견적을 만들지 않는다.
- 이번 작업은 스키마나 저장 데이터를 변경하지 않으므로 코드와 문서 변경을 되돌리는 것만으로 롤백할 수 있다.

## 검증 체크리스트

- [x] 정식 용어가 PriceQuote의 가격 보장과 재고 비보장 경계를 설명한다.
- [x] PriceQuote가 정확한 점유일의 DailyRate만 사용한다.
- [x] PriceQuote가 날짜순 NightlyPrice와 정확한 TotalPrice를 보존한다.
- [x] PriceQuote가 견적 시점의 CancellationPolicy를 보존한다.
- [x] PriceQuote가 정확한 만료 경계를 판정한다.
- [x] 유효 입력, 경계값, 거부 경로 단위 테스트가 통과한다.
- [x] 모듈 경계 아키텍처 테스트가 통과한다.
- [x] 전체 솔루션 빌드와 포맷 검증이 통과한다.

검증 명령:

```bash
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj --filter "FullyQualifiedName~Pricing.PriceQuotes"
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
dotnet test backend/tests/HotelBooking.ArchitectureTests/HotelBooking.ArchitectureTests.csproj
dotnet build backend/HotelBooking.slnx --no-restore
dotnet format backend/HotelBooking.slnx --no-restore --verify-no-changes
git diff --check
```

## 완료 기준(DoD)

- [x] `CONTEXT.md`가 Requested Occupancy와 PriceQuote의 정식 의미를 정확히 설명한다.
- [x] 구현이 이 문서의 도메인 규칙과 Aggregate 경계를 따른다.
- [x] PriceQuote가 RatePlan 및 DailyRate 구현 객체를 내부 상태로 보존하지 않는다.
- [x] 새롭거나 변경된 공개 API에 의도를 설명하는 XML 문서가 있다.
- [x] 필요한 단위 테스트, 아키텍처 테스트, 빌드와 포맷 검증이 모두 통과한다.
- [x] 영속성, 애플리케이션 조정, Reservation Price Snapshot이 이번 범위 밖이라는 점이 결과 보고에 명시된다.
