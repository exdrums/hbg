export enum JewelryType {
  Ring = 1,
  Earring = 2,
  Necklace = 3,
  Bracelet = 4,
  Piercing = 5,
  Pendant = 6,
  Brooch = 7,
  Anklet = 8,
  Other = 99
}

export const JewelryTypeLabels: { [key: number]: string } = {
  [JewelryType.Ring]: 'Ring',
  [JewelryType.Earring]: 'Earring',
  [JewelryType.Necklace]: 'Necklace',
  [JewelryType.Bracelet]: 'Bracelet',
  [JewelryType.Piercing]: 'Piercing',
  [JewelryType.Pendant]: 'Pendant',
  [JewelryType.Brooch]: 'Brooch',
  [JewelryType.Anklet]: 'Anklet',
  [JewelryType.Other]: 'Other'
};

export enum GenerationSource {
  Form = 1,
  Chat = 2
}
