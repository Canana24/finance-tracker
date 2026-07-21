import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MonthlyEvolutionChart } from './monthly-evolution-chart';

describe('MonthlyEvolutionChart', () => {
  let component: MonthlyEvolutionChart;
  let fixture: ComponentFixture<MonthlyEvolutionChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonthlyEvolutionChart],
    }).compileComponents();

    fixture = TestBed.createComponent(MonthlyEvolutionChart);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
