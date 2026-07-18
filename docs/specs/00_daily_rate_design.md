# 00. DailyRate 도메인 설계

## 목표

- 호텔의 판매 통화를 `Currency` 값 객체로 일관되게 표현한다.
- RatePlan과 DailyRate 사이의 통화 불변식을 Pricing 도메인 안에서 보호한다.
- 특정 RatePlan의 점유일별 가격을 독립적으로 생성하고 변경할 수 있는 DailyRate Aggregate를 정의한다.
- 이후 Price Quote와 Reservation Price Snapshot이 같은 통화의 일별 가격을 안전하게 합산할 기반을 마련한다.

## 적용 범위

- 도메인 용어: `CONTEXT.md`
- Property 도메인: `backend/src/HotelBooking.Modules.Property/Domain/Hotels/Hotel.cs`
- Pricing 도메인: `backend/src/HotelBooking.Modules.Pricing/Domain/References`, `backend/src/HotelBooking.Modules.Pricing/Domain/RatePlans`, `backend/src/HotelBooking.Modules.Pricing/Domain/DailyRates`
- 단위 테스트: `backend/tests/HotelBooking.UnitTests/Property`, `backend/tests/HotelBooking.UnitTests/Pricing`
- 비범위: EF Core 매핑, PostgreSQL 마이그레이션, 애플리케이션 명령, HTTP API, 대량 요금 캘린더 편집, Price Quote, Reservation Price Snapshot, 취소 위약금의 금액 계산

## 사전 조건

- `Currency`는 정규화된 세 글자 통화 코드를 표현한다.
- `Money`는 음수가 아닌 금액과 정확히 하나의 `Currency`를 함께 표현한다.
- RatePlan은 하나의 호텔과 Room Type에 속한다.
- 체크인 날짜는 첫 번째 점유일이고 체크아웃 날짜는 점유일에 포함되지 않는다.

## 단계별 적용

## 00.1 호텔 판매 통화 정리

- 정식 용어를 `Hotel Selling Currency`로 정의한다.
- `Hotel.DefaultCurrency` 문자열을 `Hotel.SellingCurrency` 타입 `Currency`로 교체한다.
- 호텔 생성 이후 판매 통화는 변경하지 않는다.
- Property 모듈은 `Currency`의 형식 검증을 중복 구현하지 않는다.

## 00.2 RatePlan 판매 통화 추가

- Pricing은 호텔 식별자와 판매 통화를 `HotelRateSettings`라는 하나의 불변 값으로 전달받는다.
- RatePlan은 `HotelRateSettings` 자체를 불변 상태로 보존한다.
- RatePlan의 호텔 식별자와 판매 통화는 별도 상태로 중복 저장하지 않고 `HotelRateSettings`에서 제공한다.
- RatePlan 생성 API는 호텔 식별자와 판매 통화를 별도 인자로 받지 않는다.
- Pricing 모듈은 Property 모듈의 Hotel 구현 타입을 참조하지 않는다.
- `HotelRateSettings`는 Property 계약 또는 Pricing 소유 프로젝션에서 얻은 호텔 정보를 Pricing 경계에서 구성해야 한다.
- `HotelRateSettings` 생성 팩터리는 Pricing 어셈블리 내부에만 공개하여 다른 모듈이 임의의 호텔 식별자와 통화를 조합하지 못하게 한다.
- 이번 도메인 범위는 두 값을 분리할 수 없게 만들며, Property에서 정보를 조회하거나 동기화하는 애플리케이션 구현은 포함하지 않는다.
- RatePlan의 판매 통화는 생성 이후 변경하지 않는다.

## 00.3 DailyRate Aggregate 구현

- DailyRate는 RatePlan 안의 무제한 컬렉션이 아니라 날짜별 독립 Aggregate Root로 구현한다.
- 식별자는 `DailyRateId`를 사용한다.
- DailyRate는 `RatePlanId`, `OccupiedDate`, `Price`를 보유한다.
- 생성 시 RatePlan을 전달받아 RatePlan 식별자와 판매 통화를 검증한다.
- 생성 이후 RatePlan과 점유일은 변경하지 않고 `ChangePrice`로 금액만 변경한다.
- 변경 가격은 기존 DailyRate와 동일한 통화를 사용해야 한다.

## 00.4 단위 테스트 작성

- HotelRateSettings가 호텔 식별자와 판매 통화를 함께 보존하고 누락 값을 거부하는지 검증한다.
- 정상 생성과 0원 가격을 검증한다.
- 기본 DailyRate ID, 누락된 RatePlan, 기본 점유일, 누락된 가격을 거부하는지 검증한다.
- RatePlan 판매 통화와 다른 가격을 거부하는지 검증한다.
- 같은 통화의 가격 변경과 다른 통화로의 변경 거부를 검증한다.
- 실패한 가격 변경이 기존 가격을 보존하는지 검증한다.

## 도메인 규칙

- 한 호텔의 RatePlan과 DailyRate는 `HotelRateSettings`를 통해 전달된 호텔 판매 통화를 공유한다.
- DailyRate 가격은 `Money`이며 음수일 수 없다.
- 0원 DailyRate는 무료 프로모션이나 보상 숙박을 막지 않기 위해 허용한다.
- DailyRate의 논리적 유일성은 `(RatePlanId, OccupiedDate)`이다.
- 과거 날짜 여부는 DailyRate가 시스템 시각을 읽어 판단하지 않는다. 과거 가격 수정 정책이 필요하면 애플리케이션 경계에서 명시적인 호텔 현지 날짜를 전달한다.
- RatePlan의 상태만으로 DailyRate의 존재 여부를 단정하지 않는다. Draft 상태에서 미래 요금을 준비할 수 있고, RatePlan 활성화는 전체 판매 가능성을 의미하지 않는다.

## Aggregate 경계와 영속성

- RatePlan에 수년 치 DailyRate를 포함하면 Aggregate 크기가 무제한으로 증가하고 서로 다른 날짜의 변경이 불필요하게 충돌한다.
- DailyRate를 날짜별 Aggregate Root로 분리하여 한 날짜의 가격 변경이 다른 날짜의 변경과 독립적으로 수행되게 한다.
- RatePlan은 Property Aggregate를 직접 참조하지 않고 Pricing 경계에서 구성한 `HotelRateSettings`만 사용한다.
- 동일한 `(RatePlanId, OccupiedDate)` 중복은 Aggregate 하나만으로 검사할 수 없으므로, 영속성 도입 시 PostgreSQL 고유 제약조건과 통합 테스트로 보장한다.
- 이번 범위에는 영속성이 없으므로 단위 테스트가 데이터베이스 유일성을 증명한다고 주장하지 않는다.

## 장애 대응/롤백

- 통화 타입 변경으로 기존 호출부가 컴파일되지 않으면 모든 호출부를 `Currency.FromCode` 사용으로 이전한다.
- RatePlan 또는 DailyRate 통화 검증이 실패하면 Aggregate를 생성하거나 변경하지 않는다.
- 이번 작업은 스키마나 저장 데이터를 변경하지 않으므로 코드 변경을 되돌리는 것만으로 롤백할 수 있다.

## 검증 체크리스트

- [x] Hotel이 `Currency` 타입의 판매 통화를 보유한다.
- [x] HotelRateSettings가 호텔 식별자와 판매 통화를 하나의 값으로 묶는다.
- [x] RatePlan이 HotelRateSettings와 그 안의 필수 판매 통화를 보유한다.
- [x] DailyRate가 RatePlan과 같은 통화의 Money만 허용한다.
- [x] DailyRate가 같은 통화 안에서 가격을 변경한다.
- [x] 유효 입력, 경계값, 거부 경로 단위 테스트가 통과한다.
- [x] 모듈 경계 아키텍처 테스트가 통과한다.
- [x] 전체 솔루션 빌드와 포맷 검증이 통과한다.

검증 명령:

```bash
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj --filter "FullyQualifiedName~Property.Hotels|FullyQualifiedName~Pricing.RatePlans|FullyQualifiedName~Pricing.DailyRates"
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
dotnet test backend/tests/HotelBooking.ArchitectureTests/HotelBooking.ArchitectureTests.csproj
dotnet build backend/HotelBooking.slnx --no-restore
dotnet format backend/HotelBooking.slnx --no-restore --verify-no-changes
git diff --check
```

## 완료 기준(DoD)

- [x] `CONTEXT.md`가 호텔 판매 통화와 DailyRate의 정식 의미를 정확히 설명한다.
- [x] 구현이 이 문서의 도메인 규칙과 Aggregate 경계를 따른다.
- [x] RatePlan 생성 API가 호텔 식별자와 판매 통화를 독립적으로 조합하지 못하게 한다.
- [x] 새롭거나 변경된 공개 API에 의도를 설명하는 XML 문서가 있다.
- [x] 필요한 단위 테스트, 아키텍처 테스트, 빌드와 포맷 검증이 모두 통과한다.
- [x] EF Core 유일성 검증이 이번 범위 밖이라는 점이 결과 보고에 명시된다.
