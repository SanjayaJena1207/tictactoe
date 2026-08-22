import { TestBed } from '@angular/core/testing';
import { ScoreboardStore } from '../../../core/state/scoreboard.store';
import { ScoreboardPanel } from './scoreboard';

describe('ScoreboardPanel', () => {
  let store: ScoreboardStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ScoreboardPanel] }).compileComponents();
    store = TestBed.inject(ScoreboardStore);
  });

  it('renders nothing before the scoreboard has loaded', () => {
    const fixture = TestBed.createComponent(ScoreboardPanel);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.scoreboard')).toBeNull();
  });

  it('displays wins and draws from the store', () => {
    store.setScoreboard({ xWins: 3, oWins: 1, draws: 2 });
    const fixture = TestBed.createComponent(ScoreboardPanel);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('X Wins');
    expect(text).toContain('3');
    expect(text).toContain('O Wins');
    expect(text).toContain('1');
    expect(text).toContain('Draws');
    expect(text).toContain('2');
  });
});
