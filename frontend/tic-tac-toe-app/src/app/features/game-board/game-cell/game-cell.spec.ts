import { TestBed } from '@angular/core/testing';
import { GameCell } from './game-cell';

describe('GameCell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameCell],
    }).compileComponents();
  });

  function createCell() {
    const fixture = TestBed.createComponent(GameCell);
    fixture.componentRef.setInput('status', 'InProgress');
    fixture.componentRef.setInput('index', 4);
    return fixture;
  }

  it('is clickable when empty and game is in progress', () => {
    const fixture = createCell();
    fixture.detectChanges();
    expect(fixture.componentInstance.isClickable()).toBe(true);
  });

  it('is not clickable when already filled', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('value', 'X');
    fixture.detectChanges();
    expect(fixture.componentInstance.isClickable()).toBe(false);
  });

  it('is not clickable when the game is no longer in progress', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('status', 'Won');
    fixture.detectChanges();
    expect(fixture.componentInstance.isClickable()).toBe(false);
  });

  it('emits its own index when clicked while clickable', () => {
    const fixture = createCell();
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));

    fixture.componentInstance.onClick();

    expect(emitted).toEqual([4]);
  });

  it('does not emit when clicked while not clickable', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('value', 'O');
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));

    fixture.componentInstance.onClick();

    expect(emitted).toEqual([]);
  });
});
