import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioDashboard } from './portfolio-dashboard';

describe('PortfolioDashboard', () => {
  let component: PortfolioDashboard;
  let fixture: ComponentFixture<PortfolioDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioDashboard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
