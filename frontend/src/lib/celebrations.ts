export interface CelebrationInfo {
  esCumpleanos: boolean;
  esAniversario: boolean;
  aniosAniversario: number | null;
}

function parseDateOnly(value: string | null): { year: number; month: number; day: number } | null {
  if (!value) return null;
  const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
  if (!match) return null;
  return { year: Number(match[1]), month: Number(match[2]), day: Number(match[3]) };
}

export function getCelebrationInfo(
  fechaNac: string | null,
  fechaIng: string | null,
  today: Date = new Date(),
): CelebrationInfo {
  const todayMonth = today.getMonth() + 1;
  const todayDay = today.getDate();

  const nac = parseDateOnly(fechaNac);
  const esCumpleanos = !!nac && nac.month === todayMonth && nac.day === todayDay;

  const ing = parseDateOnly(fechaIng);
  let esAniversario = false;
  let aniosAniversario: number | null = null;
  if (ing && ing.month === todayMonth && ing.day === todayDay) {
    const anios = today.getFullYear() - ing.year;
    if (anios >= 1) {
      esAniversario = true;
      aniosAniversario = anios;
    }
  }

  return { esCumpleanos, esAniversario, aniosAniversario };
}
