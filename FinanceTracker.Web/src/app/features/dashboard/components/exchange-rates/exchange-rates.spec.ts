import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExchangeRates } from './exchange-rates';

describe('ExchangeRates', () => {
  let component: ExchangeRates;
  let fixture: ComponentFixture<ExchangeRates>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExchangeRates],
    }).compileComponents();

    fixture = TestBed.createComponent(ExchangeRates);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
