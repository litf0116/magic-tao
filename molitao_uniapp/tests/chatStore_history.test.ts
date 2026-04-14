import { describe, it, expect } from 'vitest'
import { mergeHistoryForChannel } from '../src/stores/chatStore'

describe('mergeHistoryForChannel (system channel history merge)', () => {
  it('merges new items with existing keeping chronological order', () => {
    const newItems: any[] = [
      { id: 101, time: 1000 },
      { id: 102, time: 2000 }
    ]
    const existing: any[] = [
      { id: 1, time: 1500 }
    ]
    const res = mergeHistoryForChannel('-10_announcement', newItems as any, existing as any, false)
    // Expect merged length 3 and time-sorted order: 1000, 1500, 2000
    expect(res.length).toBe(3)
    expect(res[0].time).toBe(1000)
  })
})
