/**
 * 创建一个可重用的倒计时钩子
 * @param initialTargetDate 初始目标日期字符串
 * @returns 包含倒计时状态和控制方法的对象
 */
export function useCountdown(initialTargetDate: string) {
    // 创建响应式变量
    const targetDate = ref(initialTargetDate);
    const days = ref(0);
    const hours = ref('00');
    const minutes = ref('00');
    const seconds = ref('00');
    const isFinished = ref(false);

    // 用于存储定时器ID
    let timer: number | null = null;

    /**
     * 倒计时的核心函数
     */
    const countdown = (): void => {
        // 检查目标日期是否有效，如果无效则设置为结束状态
        if (targetDate.value === "0001-01-01 00:00:00") {
            setFinishedState();
            return;
        }

        // 计算目标时间和当前时间的时间戳差
        const end = Date.parse(new Date(targetDate.value).toString());
        const now = Date.now();
        const msec = end - now;

        // 如果时间差小于0，说明已经过了目标时间，设置为结束状态
        if (msec < 0) {
            setFinishedState();
            return;
        }

        // 计算剩余的天、时、分、秒
        const d = Math.floor(msec / (1000 * 60 * 60 * 24));
        const h = Math.floor((msec / (1000 * 60 * 60)) % 24);
        const m = Math.floor((msec / (1000 * 60)) % 60);
        const s = Math.floor((msec / 1000) % 60);

        // 更新响应式变量
        days.value = d;
        hours.value = h > 9 ? h.toString() : "0" + h;
        minutes.value = m > 9 ? m.toString() : "0" + m;
        seconds.value = s > 9 ? s.toString() : "0" + s;
        isFinished.value = false;

        // 清除之前的定时器（如果存在），并设置新的定时器
        if (timer) clearTimeout(timer);
        timer = setTimeout(countdown, 1000);
    };

    /**
     * 设置倒计时结束状态
     */
    const setFinishedState = () => {
        days.value = 0;
        hours.value = "00";
        minutes.value = "00";
        seconds.value = "00";
        isFinished.value = true;
        if (timer) clearTimeout(timer);
    };

    /**
     * 开始倒计时
     */
    const startCountdown = () => {
		 
        countdown();
    };

    /**
     * 停止倒计时
     */
    const stopCountdown = () => {
        if (timer) clearTimeout(timer);
    };

    /**
     * 重置倒计时
     * @param newTargetDate 新的目标日期字符串
     */
    const resetCountdown = (newTargetDate: string) => {
        stopCountdown(); // 先停止当前的倒计时
        targetDate.value = newTargetDate; // 设置新的目标日期
        startCountdown(); // 重新开始倒计时
    };

    // 返回包含倒计时状态和控制方法的对象
    return {
        days,
        hours,
        minutes,
        seconds,
        isFinished,
        startCountdown,
        stopCountdown,
        resetCountdown
    };
}